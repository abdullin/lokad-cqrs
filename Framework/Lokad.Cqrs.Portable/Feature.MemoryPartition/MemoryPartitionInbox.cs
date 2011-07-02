#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionInbox : IPartitionInbox
    {
        readonly BlockingCollection<ImmutableEnvelope>[] _queues;
        readonly string[] _names;

        public MemoryPartitionInbox(BlockingCollection<ImmutableEnvelope>[] queues, string[] names)
        {
            _queues = queues;
            _names = names;
        }

        public void Init()
        {
        }

        public void AckMessage(EnvelopeTransportContext envelope)
        {
            
        }

        public bool TakeMessage(CancellationToken token, out EnvelopeTransportContext context)
        {
            ImmutableEnvelope envelope;

            while (!token.IsCancellationRequested)
            {
                // if incoming message is delayed and in future -> push it to the timer queue.
                // timer will be responsible for publishing back.

                var result = BlockingCollection<ImmutableEnvelope>.TakeFromAny(_queues, out envelope);
                if (result >= 0)
                {
                    if (envelope.DeliverOnUtc > DateTime.UtcNow)
                    {
                        // future message
                        throw new InvalidOperationException("Message scheduling has been disabled in the code");
                    }
                    context = new EnvelopeTransportContext(result, envelope, _names[result]);
                    return true;
                }
            }
            context = null;
            return false;
        }

        public void TryNotifyNack(EnvelopeTransportContext context)
        {
            var id = (int) context.TransportMessage;

            _queues[id].Add(context.Unpacked);
        }
    }
}