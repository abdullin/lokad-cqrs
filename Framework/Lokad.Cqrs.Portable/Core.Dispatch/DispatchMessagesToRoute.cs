#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Core.Dispatch
{
    ///<summary>
    /// Simple dispatcher that forwards messages according to the rules.
    /// We don't care about duplication management, since recipients will do that.
    ///</summary>
    public sealed class DispatchMessagesToRoute : ISingleThreadMessageDispatcher
    {
        readonly QueueWriterRegistry _queueRegistry;
        readonly Func<ImmutableEnvelope, string> _routerRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchMessagesToRoute"/> class.
        /// </summary>
        /// <param name="queueRegistry">The queue registry.</param>
        /// <param name="routingRules">The routing rules.</param>
        public DispatchMessagesToRoute(QueueWriterRegistry queueRegistry, Func<ImmutableEnvelope,string> routingRules)
        {
            _queueRegistry = queueRegistry;
            _routerRule = routingRules;
        }


        /// <summary>
        /// Dispatches the message by forwarding it according to the routing rules.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DispatchMessage(ImmutableEnvelope message)
        {
            var route = _routerRule(message);

            // allow discarding messages, if explicitly allowed
            if (route == "memory:null")
                return;


            var items = route.Split(new[] {':'}, 2);
            string queueName;
            string endpoint;

            if (items.Length == 1)
            {
                queueName = route;
                endpoint = "default";
            }
            else
            {
                endpoint = items[0];
                queueName = items[1];
            }

            IQueueWriterFactory factory;
            if (!_queueRegistry.TryGet(endpoint, out factory))
            {
                var s = string.Format(
                        "Route '{0}' was not handled by any single dispatcher. Did you want to send to 'memory:null' or 'memory:{0}' instead? Please, read documentation on Routing Dispatcher.",
                        route);
                throw new InvalidOperationException(s);
            }
            factory.GetWriteQueue(queueName).PutMessage(message);
            return;
        }

        /// <summary>
        /// Initializes this instance
        /// </summary>
       public void Init()
        {
            if (null == _routerRule)
            {
                throw new InvalidOperationException("Message router must be configured!");
            }
        }
    }
}