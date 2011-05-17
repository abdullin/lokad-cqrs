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
        readonly IEnvelopeQuarantine _quarantine;

        public DispatcherProcess(ISystemObserver observer,
            ISingleThreadMessageDispatcher dispatcher, 
            IPartitionInbox inbox,
            IEnvelopeQuarantine quarantine,
            MessageDuplicationManager manager)
        {
            _dispatcher = dispatcher;
            _quarantine = quarantine;
            _observer = observer;
            _inbox = inbox;
            _memory = manager.GetOrAdd(this);
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
        readonly MessageDuplicationMemory _memory;

        public Task Start(CancellationToken token)
        {
            return Task.Factory
                .StartNew(() =>
                    {
                        try
                        {
                            ReceiveMessages(token);
                        }
                        catch(ObjectDisposedException)
                        {
                            // suppress
                        }
                    }, token);
        }


        void ReceiveMessages(CancellationToken outer)
        {
            using (var source = CancellationTokenSource.CreateLinkedTokenSource(_disposal.Token, outer))
            {
                EnvelopeTransportContext context;
                while (_inbox.TakeMessage(source.Token, out context))
                {
                    try
                    {
                        ProcessMessage(context);
                    }
                    catch(Exception ex)
                    {
                        var e = new DispatchRecoveryFailed(ex, context.Unpacked, context.QueueName);
                        _observer.Notify(e);
                    }
                }
            }
        }

        void ProcessMessage(EnvelopeTransportContext context) 
        {
            var processed = false;
            try
            {
                if (!_memory.DoWeRemember(context.Unpacked.EnvelopeId))
                {
                    _dispatcher.DispatchMessage(context.Unpacked);
                    _memory.Memorize(context.Unpacked.EnvelopeId);
                }
                
                processed = true;
            }
            catch (Exception dispatchEx)
            {
                // if the code below fails, it will just cause everything to be reprocessed later,
                // which is OK (duplication manager will handle this)

                _observer.Notify(new EnvelopeDispatchFailed(context.Unpacked, context.QueueName, dispatchEx));
                // quarantine is atomic with the processing
                if (_quarantine.TryToQuarantine(context, dispatchEx))
                {
                    _observer.Notify(new EnvelopeQuarantined(dispatchEx, context.Unpacked, context.QueueName));
                    // acking message is the last step!
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
                    // 1st step - dequarantine, if present
                    _quarantine.TryRelease(context);
                    // 2nd step - ack.
                    _inbox.AckMessage(context);
                    // 3rd - notify.
                    _observer.Notify(new EnvelopeAcked(context.QueueName, context.Unpacked.EnvelopeId, context.Unpacked.GetAllAttributes()));
                }
            }
            catch (Exception ex)
            {
                // not a big deal. Message will be processed again.
                _observer.Notify(new FailedToAckEnvelope(ex, context.Unpacked.EnvelopeId, context.QueueName));
            }
        }
    }
}