#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Core.Directory;

namespace Lokad.Cqrs.Core.Dispatch
{
    /// <summary>
    /// Dispatch command batches to a single consumer. Uses sliding cache to 
    /// reduce message duplication
    /// </summary>
    public class DispatchCommandBatch : ISingleThreadMessageDispatcher
    {
        readonly ILifetimeScope _container;
        readonly IDictionary<Type, Type> _messageConsumers = new Dictionary<Type, Type>();
        readonly MessageActivationMap _messageDirectory;
        readonly IMethodInvoker _invoker;

        public DispatchCommandBatch(
            ILifetimeScope container, 
            MessageActivationMap messageDirectory,
            IMethodInvoker invoker)
        {
            _container = container;
            _invoker = invoker;
            _messageDirectory = messageDirectory;
        }

        public void DispatchMessage(ImmutableEnvelope message)
        {
            // empty message, hm...
            if (message.Items.Length == 0)
                return;

            // verify that all consumers are available
            foreach (var item in message.Items)
            {
                if (!_messageConsumers.ContainsKey(item.MappedType))
                {
                    throw new InvalidOperationException("Couldn't find consumer for " + item.MappedType);
                }
            }
            DispatchEnvelope(message);
        }

        protected virtual void DispatchEnvelope(ImmutableEnvelope message)
        {
            using (var unit = _container.BeginLifetimeScope(DispatchLifetimeScopeTags.MessageEnvelopeScopeTag))
            {
                foreach (var item in message.Items)
                {
                    // we're dispatching them inside single lifetime scope
                    // meaning same transaction,
                    using (var scope = unit.BeginLifetimeScope(DispatchLifetimeScopeTags.MessageItemScopeTag))
                    {
                        var consumerType = _messageConsumers[item.MappedType];
                        {
                            var consumer = scope.Resolve(consumerType);
                            _invoker.InvokeConsume(consumer, item, message);
                        }
                    }
                }
            }
        }


        public void Init()
        {
            var infos = _messageDirectory.Infos;
            DispatcherUtil.ThrowIfCommandHasMultipleConsumers(infos);
            foreach (var messageInfo in infos)
            {
                if (messageInfo.AllConsumers.Length > 0)
                {
                    _messageConsumers[messageInfo.MessageType] = messageInfo.AllConsumers[0];
                }
            }
        }
    }
}