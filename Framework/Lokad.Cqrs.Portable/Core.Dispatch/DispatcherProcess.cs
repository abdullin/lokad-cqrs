#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Core.Dispatch
{
    public sealed class DispatcherProcess : IEngineProcess
    {
        readonly ISingleThreadMessageDispatcher _dispatcher;
        readonly ISystemObserver _observer;
        readonly IPartitionInbox _inbox;
        readonly MemoryQuarantine _quarantine;

        public DispatcherProcess(ISystemObserver observer,
            ISingleThreadMessageDispatcher dispatcher, IPartitionInbox inbox, MemoryQuarantine quarantine)
        {
            _dispatcher = dispatcher;
            _quarantine = quarantine;
            _observer = observer;
            _inbox = inbox;
        }

        public void Dispose()
        {
            _disposal.Dispose();
        }

        public void Initialize()
        {
            _inbox.Init();
        }

        readonly CancellationTokenSource _disposal = new CancellationTokenSource();

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() => ReceiveMessages(token), token);
        }


        void ReceiveMessages(CancellationToken outer)
        {
            using (var source = CancellationTokenSource.CreateLinkedTokenSource(_disposal.Token, outer))
            {
                var token = source.Token;
                MessageContext context;
                while (_inbox.TakeMessage(token, out context))
                {
                    var processed = false;
                    try
                    {
                        _dispatcher.DispatchMessage(context.Unpacked);
                        processed = true;
                    }
                    catch (Exception ex)
                    {
                        _observer.Notify(new FailedToConsumeMessage(ex, context.Unpacked.EnvelopeId, context.QueueName));
                        // if the code below fails, it will just cause everything to be reprocessed later
                        if (_quarantine.Accept(context, ex))
                        {
                            _observer.Notify(new QuarantinedMessage(ex, context.Unpacked.EnvelopeId, context.QueueName));
                            _inbox.AckMessage(context);
                        }
                        else
                        {
                            _inbox.TryNotifyNack(context);
                        }
                    }
                    try
                    {
                        if (processed)
                        {
                            _inbox.AckMessage(context);
                            _quarantine.Clear(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        // not a big deal. Message will be processed again.
                        _observer.Notify(new FailedToAckMessage(ex, context.Unpacked.EnvelopeId, context.QueueName));
                    }
                }
            }
        }
    }
}