#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
    /// <summary>
    /// Handles deserialization, joins multiple queues and notifications
    /// </summary>
    public sealed class AzurePartitionInbox : IPartitionInbox
    {
        readonly StatelessAzureQueueReader[] _readers;
        readonly StatelessAzureFutureList[] _futures;

        public AzurePartitionInbox(StatelessAzureQueueReader[] readers, StatelessAzureFutureList[] futures,
            Func<uint, TimeSpan> waiter)
        {
            _readers = readers;
            _futures = futures;
            _waiter = waiter;
        }

        public void Init()
        {
            foreach (var queue in _readers)
            {
                queue.Initialize();
            }
            foreach (var future in _futures)
            {
                future.Initialize();
            }
        }

        public void AckMessage(EnvelopeTransportContext envelope)
        {
            foreach (var queue in _readers)
            {
                if (queue.Name == envelope.QueueName)
                {
                    queue.AckMessage(envelope);
                }
            }
        }

        public void TryNotifyNack(EnvelopeTransportContext context)
        {
        }

        readonly Func<uint, TimeSpan> _waiter;
        uint _emptyCycles;

        public bool TakeMessage(CancellationToken token, out EnvelopeTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < _readers.Length; i++)
                {
                    var queue = _readers[i];
                    var future = _futures[i];

                    var message = queue.TryGetMessage();
                    switch (message.State)
                    {
                        case GetEnvelopeResultState.Success:

                            _emptyCycles = 0;
                            // future message
                            if (message.Envelope.Unpacked.DeliverOn > DateTimeOffset.UtcNow)
                            {
                                // save
                                future.PutMessage(message.Envelope.Unpacked);
                                queue.AckMessage(message.Envelope);
                                break;
                            }
                            context = message.Envelope;
                            return true;
                        case GetEnvelopeResultState.Empty:
                            _emptyCycles += 1;
                            break;
                        case GetEnvelopeResultState.Exception:
                            // access problem, fall back a bit
                            break;
                        case GetEnvelopeResultState.Retry:
                            // this could be the poison
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    var waiting = _waiter(_emptyCycles);
                    token.WaitHandle.WaitOne(waiting);
                }
            }
            context = null;
            return false;
        }
    }
}