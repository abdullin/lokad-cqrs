using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryQueueFactory : IReadQueueFactory, IWriteQueueFactory
	{
		readonly ConcurrentDictionary<string,ConcurrentQueue<MessageEnvelope>> _factory = new ConcurrentDictionary<string, ConcurrentQueue<MessageEnvelope>>();

		public IReadQueue GetReadQueue(string name)
		{
			var queue = _factory.GetOrAdd(name, s=> new ConcurrentQueue<MessageEnvelope>());
			return new MemoryReadQueue(queue);
		}

		public IWriteQueue GetWriteQueue(string name)
		{
			var queue = _factory.GetOrAdd(name, s => new ConcurrentQueue<MessageEnvelope>());
			return new MemoryWriteQueue(queue);
		}
	}
}