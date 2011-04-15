using System;
using System.Threading;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	/// <summary>
	/// Handles deserialization, joins multiple queues and notifications
	/// </summary>
	public sealed class AzurePartitionNotifier : IPartitionScheduler
	{
		readonly AzureReadQueue[] _azureQueues;

		public AzurePartitionNotifier(AzureReadQueue[] azureQueues, Func<uint, TimeSpan> waiter)
		{
			_azureQueues = azureQueues;
			_waiter = waiter;
		}

		public void Init()
		{
			foreach (var queue in _azureQueues)
			{
				queue.Init();
			}
		}

		public void AckMessage(MessageContext message)
		{
			foreach (var queue in _azureQueues)
			{
				if (queue.Name == message.QueueName)
				{
					queue.AckMessage(message);
				}
			}
		}
		
		public void TryNotifyNack(MessageContext context)
		{
		}

		readonly Func<uint, TimeSpan> _waiter;
		uint _emptyCycles;

		public bool TakeMessage(CancellationToken token, out MessageContext context)
		{
			while(!token.IsCancellationRequested)
			{
				foreach (var queue in _azureQueues)
				{
					var message = queue.TryGetMessage();
					switch (message.State)
					{
						case GetMessageResultState.Success:
							context = message.Message;
							_emptyCycles = 0;
							return true;
						case GetMessageResultState.Empty:
							_emptyCycles += 1;
							break;
						case GetMessageResultState.Exception:
							// access problem, fall back a bit
							break;
						case GetMessageResultState.Retry:
							// this could be the poison
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					var waiting = _waiter(_emptyCycles);
					token.WaitHandle.WaitOne(waiting);
				}
			}
			context = null;
			return false;
		}
	}
}