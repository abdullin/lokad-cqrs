#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Linq;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionFactory
    {
        readonly MemoryAccount _account;

        public MemoryPartitionFactory(MemoryAccount account)
        {
            _account = account;
        }


        public MemoryPartitionInbox GetMemoryInbox(string[] queueNames)
        {
            var queues = queueNames
                .Select(n => _account.Delivery.GetOrAdd(n, s => new BlockingCollection<ImmutableEnvelope>()))
                .ToArray();

            var pending = queueNames
                .Select(n => _account.Pending.GetOrAdd(n, s => new MemoryFutureList()))
                .ToArray();

            return new MemoryPartitionInbox(queues, queueNames, pending);
        }
    }
}