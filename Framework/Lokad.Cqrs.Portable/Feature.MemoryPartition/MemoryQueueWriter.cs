#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryQueueWriter : IQueueWriter
    {
        readonly BlockingCollection<ImmutableMessageEnvelope> _queue;

        public MemoryQueueWriter(BlockingCollection<ImmutableMessageEnvelope> queue)
        {
            _queue = queue;
        }

        public void PutMessage(ImmutableMessageEnvelope envelope)
        {
            _queue.Add(envelope);
        }

        public void Init()
        {
        }
    }
}