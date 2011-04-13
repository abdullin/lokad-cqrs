using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryWriteQueue : IWriteQueue
	{
		readonly ConcurrentQueue<MessageEnvelope> _queue;

		public MemoryWriteQueue(ConcurrentQueue<MessageEnvelope> queue)
		{
			_queue = queue;
		}

		public void SendMessage(MessageEnvelope envelope)
		{
			_queue.Enqueue(envelope);
		}

		public void Init()
		{
			
		}
	}
}