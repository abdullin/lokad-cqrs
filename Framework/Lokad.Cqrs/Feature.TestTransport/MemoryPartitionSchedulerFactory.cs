using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Transport;
using System.Linq;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryPartitionSchedulerFactory : IPartitionSchedulerFactory, IWriteQueueFactory
	{
		readonly ConcurrentDictionary<string, BlockingCollection<MessageEnvelope>> _factory = new ConcurrentDictionary<string, BlockingCollection<MessageEnvelope>>();

		public IWriteQueue GetWriteQueue(string name)
		{
			var queue = _factory.GetOrAdd(name, s => new BlockingCollection<MessageEnvelope>());
			return new MemoryWriteQueue(queue);
		}

		public IPartitionScheduler GetNotifier(string[] queueNames)
		{
			var blockers = queueNames
				.Select(n => _factory.GetOrAdd(n, s => new BlockingCollection<MessageEnvelope>()))
				.ToArray();

			return new MemoryPartition(blockers, queueNames);
		}
	}
}