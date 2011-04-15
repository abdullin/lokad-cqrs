#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryPartition : IPartitionInbox
	{
		readonly BlockingCollection<MessageEnvelope>[] _queues;
		readonly string[] _names;
		readonly MemoryPendingList[] _future;

		public MemoryPartition(BlockingCollection<MessageEnvelope>[] queues, string[] names, MemoryPendingList[] future)
		{
			_queues = queues;
			_names = names;
			_future = future;
		}

		public void Init()
		{
		}

		public void AckMessage(MessageContext message)
		{
			// do nothing
		}

		public bool TakeMessage(CancellationToken token, out MessageContext context)
		{
			MessageEnvelope envelope;

			while (!token.IsCancellationRequested)
			{
				// if incoming message is delayed and in future -> push it to the timer queue.
				// timer will be responsible for publishing back.

				var result = BlockingCollection<MessageEnvelope>.TakeFromAny(_queues, out envelope);
				if (result >= 0)
				{
					if (envelope.DeliverOn > DateTimeOffset.UtcNow)
					{
						// future message
						_future[result].PutMessage(envelope);
						continue;
					}
					context = new MessageContext(result, envelope, _names[result]);
					return true;
				}

			}
			context = null;
			return false;
		}

		public void TryNotifyNack(MessageContext context)
		{
			var id = (int) context.TransportMessage;

			_queues[id].Add(context.Unpacked);
		}
	}
}