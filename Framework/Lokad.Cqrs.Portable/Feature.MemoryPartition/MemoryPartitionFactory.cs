#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionFactory : IQueueWriterFactory, IEngineProcess
    {
        readonly ConcurrentDictionary<string, BlockingCollection<ImmutableMessageEnvelope>> _delivery =
            new ConcurrentDictionary<string, BlockingCollection<ImmutableMessageEnvelope>>();

        readonly ConcurrentDictionary<string, MemoryFutureList> _pending =
            new ConcurrentDictionary<string, MemoryFutureList>();


        public bool TryGetWriteQueue(string endpoint, string queueName, out IQueueWriter writer)
        {
            if (endpoint != "memory")
            {
                writer = null;
                return false;
            }
            
            var queue = _delivery.GetOrAdd(queueName, s => new BlockingCollection<ImmutableMessageEnvelope>());
            writer = new MemoryQueueWriter(queue);
            return true;
        }

        public MemoryPartitionInbox GetMemoryInbox(string[] queueNames)
        {
            var queues = queueNames
                .Select(n => _delivery.GetOrAdd(n, s => new BlockingCollection<ImmutableMessageEnvelope>()))
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
                            ImmutableMessageEnvelope envelope;
                            while (list.Value.TakePendingMessage(out envelope))
                            {
                                _delivery
                                    .GetOrAdd(list.Key, n => new BlockingCollection<ImmutableMessageEnvelope>())
                                    .Add(envelope);
                            }
                        }

                        token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    }
                });
        }
    }
}