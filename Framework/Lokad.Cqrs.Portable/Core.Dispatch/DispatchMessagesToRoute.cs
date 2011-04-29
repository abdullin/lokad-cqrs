#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Core.Dispatch
{
    ///<summary>
    /// Simple dispatcher that forwards messages according to the rules.
    /// We don't care about duplication management, since recipients will do that.
    ///</summary>
    public sealed class DispatchMessagesToRoute : ISingleThreadMessageDispatcher
    {
        readonly IEnumerable<IQueueWriterFactory> _queueFactory;
        Func<ImmutableMessageEnvelope, string> _routerRule;

        public DispatchMessagesToRoute(IEnumerable<IQueueWriterFactory> queueFactory)
        {
            // static implementation for now
            _queueFactory = queueFactory.ToArray();
        }


        public void DispatchMessage(ImmutableMessageEnvelope message)
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

            int routed = 0;

            foreach (var factory in _queueFactory)
            {
                IQueueWriter writer;
                if (factory.TryGetWriteQueue(endpoint, queueName, out writer))
                {
                    writer.PutMessage(message);
                    routed += 1;
                }
            }

            if (routed == 0)
                throw new InvalidOperationException(string.Format("Route '{0}' was not handled by any single dispatcher. Did you want to send to 'memory:null' instead?", route));
        }

        public void SpecifyRouter(Func<ImmutableMessageEnvelope, string> router)
        {
            _routerRule = router;
        }

        public void Init()
        {
            if (null == _routerRule)
            {
                throw new InvalidOperationException("Message router must be configured!");
            }
        }
    }
}