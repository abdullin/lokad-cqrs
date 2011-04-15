using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestPartition
{
	public sealed class MemoryQueueWriter : IQueueWriter
	{
		readonly BlockingCollection<MessageEnvelope> _queue;

		public MemoryQueueWriter(BlockingCollection<MessageEnvelope> queue)
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