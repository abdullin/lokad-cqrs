using System;
using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Core.Partition.Testable
{
	public sealed class MemoryWriteQueue : IWriteQueue
	{
		readonly BlockingCollection<MessageEnvelope> _queue;

		public MemoryWriteQueue(BlockingCollection<MessageEnvelope> queue)
		{
			_queue = queue;
		}

		public void SendMessage(MessageEnvelope envelope)
		{
			_queue.Add(envelope);
		}

		public void Init()
		{
			
		}
	}
}