using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryWriteQueue : IQueueWriter
	{
		readonly BlockingCollection<MessageEnvelope> _queue;

		public MemoryWriteQueue(BlockingCollection<MessageEnvelope> queue)
		{
			_queue = queue;
		}

		public void PutMessage(MessageEnvelope envelope)
		{
			_queue.Add(envelope);
		}

		public void Init()
		{
			
		}
	}
}