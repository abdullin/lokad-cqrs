#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryPartitionFactory : IPartitionInboxFactory, IWriteQueueFactory, IEngineProcess
	{
		readonly ConcurrentDictionary<string, BlockingCollection<MessageEnvelope>> _delivery =
			new ConcurrentDictionary<string, BlockingCollection<MessageEnvelope>>();

		readonly ConcurrentDictionary<string,MemoryFutureList> _pending = new ConcurrentDictionary<string, MemoryFutureList>();

		public IQueueWriter GetWriteQueue(string name)
		{
			var queue = _delivery.GetOrAdd(name, s => new BlockingCollection<MessageEnvelope>());
			return new MemoryQueueWriter(queue);
		}

		public IPartitionInbox GetNotifier(string[] queueNames)
		{
			var queues = queueNames
				.Select(n => _delivery.GetOrAdd(n, s => new BlockingCollection<MessageEnvelope>()))
				.ToArray();

			var pending = queueNames
				.Select(n => _pending.GetOrAdd(n, s => new MemoryFutureList()))
				.ToArray();

			return new MemoryPartitionInbox(queues, queueNames, pending);
		}

		public void Dispose()
		{
			
		}

		public void Initialize()
		{
			
		}

		public Task Start(CancellationToken token)
		{
			return Task.Factory.StartNew(() =>
				{
					while (!token.IsCancellationRequested)
					{
						foreach (var list in _pending)
						{
							MessageEnvelope envelope;
							while(list.Value.TakePendingMessage(out envelope))
							{
								_delivery
									.GetOrAdd(list.Key, n => new BlockingCollection<MessageEnvelope>())
									.Add(envelope);
							}
						}

						token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
					}
				});
		}
	}
}