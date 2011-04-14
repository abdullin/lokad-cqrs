#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Linq;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryPartitionSchedulerFactory : IPartitionSchedulerFactory, IWriteQueueFactory
	{
		readonly ConcurrentDictionary<string, BlockingCollection<MessageEnvelope>> _factory =
			new ConcurrentDictionary<string, BlockingCollection<MessageEnvelope>>();


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