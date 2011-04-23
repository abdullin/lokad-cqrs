using System;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
	/// <summary>
	/// Handles deserialization, joins multiple queues and notifications
	/// </summary>
	public sealed class AzurePartitionInbox : IPartitionInbox
	{
		readonly StatelessAzureQueueReader[] _readers;
		readonly StatelessAzureFutureList[] _futures;

		public AzurePartitionInbox(StatelessAzureQueueReader[] readers, StatelessAzureFutureList[] futures, Func<uint, TimeSpan> waiter)
		{
			_readers = readers;
			_futures = futures;
			_waiter = waiter;
		}

		public void Init()
		{
			foreach (var queue in _readers)
			{
				queue.Initialize();
			}
			foreach (var future in _futures)
			{
				future.Initialize();
			}
		}

		public void AckMessage(MessageContext message)
		{
			foreach (var queue in _readers)
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
				for (int i = 0; i < _readers.Length; i++)
				{
					var queue = _readers[i];
					var future = _futures[i];

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