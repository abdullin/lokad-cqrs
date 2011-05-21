#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch.Events;

namespace Lokad.Cqrs.Core.Dispatch
{
    ///<summary>
    /// Dispatcher that sends a single event to multiple consumers within this worker.
    /// No transactions are used here, we keep track of duplication.
    ///</summary>
    public sealed class DispatchOneEvent : ISingleThreadMessageDispatcher
    {
        readonly MessageActivationMap _directory;
        readonly IDictionary<Type, Type[]> _dispatcher = new Dictionary<Type, Type[]>();
        readonly ISystemObserver _observer;


        readonly IMessageDispatchStrategy _strategy;

        public DispatchOneEvent(
            MessageActivationMap directory,
            ISystemObserver observer,
            IMessageDispatchStrategy strategy)
        {
            _observer = observer;
            _directory = directory;
            _strategy = strategy;
        }


        public void Init()
        {
            foreach (var message in _directory.Infos)
            {
                if (message.AllConsumers.Length > 0)
                {
                    _dispatcher.Add(message.MessageType, message.AllConsumers);
                }
            }
        }

        public void DispatchMessage(ImmutableEnvelope envelope)
        {
            if (envelope.Items.Length != 1)
                throw new InvalidOperationException(
                    "Batch message arrived to the shared scope. Are you batching events or dispatching commands to shared scope?");

            // we get failure if one of the subscribers fails
            // hence, if any of the handlers fail - we give up
            var item = envelope.Items[0];
            Type[] consumerTypes;

            if (!_dispatcher.TryGetValue(item.MappedType, out consumerTypes))
            {
                // else -> we don't have consumers. It's OK for the event
                _observer.Notify(new EventHadNoConsumers(envelope.EnvelopeId, item.MappedType));
                return;
            }

            using (var scope = _strategy.BeginEnvelopeScope())
            {
                foreach (var consumerType in consumerTypes)
                {
                    scope.Dispatch(consumerType, envelope, item);
                }
                scope.Complete();
            }
        }
    }
}