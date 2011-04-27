#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch.Events;

namespace Lokad.Cqrs.Core.Dispatch
{
    ///<summary>
    /// Dispatcher that sends a single event to multiple consumers within this worker.
    /// No transactions are used here, we keep track of duplication.
    ///</summary>
    public sealed class DispatchEventToMultipleConsumers : ISingleThreadMessageDispatcher
    {
        readonly ILifetimeScope _container;
        readonly MessageDirectory _directory;
        readonly IDictionary<Type, Type[]> _dispatcher = new Dictionary<Type, Type[]>();
        readonly MessageDuplicationMemory _dispatchMemory;
        readonly ISystemObserver _observer;

        public DispatchEventToMultipleConsumers(ILifetimeScope container, MessageDirectory directory,
            MessageDuplicationManager memory, ISystemObserver observer)
        {
            _container = container;
            _observer = observer;
            _directory = directory;
            _dispatchMemory = memory.GetOrAdd(this);
        }

        public void Init()
        {
            foreach (var message in _directory.Messages)
            {
                if (message.AllConsumers.Length > 0)
                {
                    _dispatcher.Add(message.MessageType, message.AllConsumers);
                }
            }
        }

        public void DispatchMessage(MessageEnvelope unpacked)
        {
            if (_dispatchMemory.DoWeRemember(unpacked.EnvelopeId))
                return;

            if (unpacked.Items.Length != 1)
                throw new InvalidOperationException(
                    "Batch message arrived to the shared scope. Are you batching events or dispatching commands to shared scope?");

            // we get failure if one of the subscribers fails
            Type[] consumerTypes;

            var item = unpacked.Items[0];
            if (_dispatcher.TryGetValue(item.MappedType, out consumerTypes))
            {
                using (var unit = _container.BeginLifetimeScope(DispatcherUtil.UnitOfWorkTag))
                {
                    foreach (var consumerType in consumerTypes)
                    {
                        using (var scope = unit.BeginLifetimeScope(DispatcherUtil.ScopeTag))
                        {
                            var consumer = scope.Resolve(consumerType);
                            _directory.InvokeConsume(consumer, item.Content);
                        }
                    }
                }
            }
            else
            {
                _observer.Notify(new EventHadNoConsumers(unpacked.EnvelopeId, item.MappedType));
            }
            // else -> we don't have consumers. It's OK for the event

            _dispatchMemory.Memorize(unpacked.EnvelopeId);
        }
    }
}