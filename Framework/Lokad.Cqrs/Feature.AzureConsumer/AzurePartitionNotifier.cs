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
		readonly AzureFutureQueue[] _azureFutures;

		public AzurePartitionNotifier(AzureReadQueue[] azureQueues, AzureFutureQueue[] azureFutures, Func<uint, TimeSpan> waiter)
		{
			_azureQueues = azureQueues;
			_azureFutures = azureFutures;
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
				for (int i = 0; i < _azureQueues.Length; i++)
				{
					var queue = _azureQueues[i];
					var future = _azureFutures[i];

					var message = queue.TryGetMessage();
					switch (message.State)
					{
						case GetMessageResultState.Success:
							
							_emptyCycles = 0;
							// future message
							if (message.Message.Unpacked.DeliverOn > DateTimeOffset.UtcNow)
							{
								// save
								future.PutMessage(message.Message.Unpacked);
								queue.AckMessage(message.Message);
								break;
							}
							context = message.Message;
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