#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class SendMessageModule : HideObjectMembersFromIntelliSense
    {
        readonly string _queueName;
        readonly string _endpoint;
        Func<string> _keyGenerator = () => Guid.NewGuid().ToString().ToLowerInvariant();

        public SendMessageModule(string endpoint, string queueName)
        {
            _queueName = queueName;
            _endpoint = endpoint;
        }

        public void IdGenerator(Func<string> generator)
        {
            _keyGenerator = generator;
        }

        public void IdGeneratorForTests()
        {
            long id = 0;
            IdGenerator(() =>
                {
                    Interlocked.Increment(ref id);
                    return id.ToString("0000");
                });
        }

        IMessageSender BuildDefaultMessageSender(IComponentContext c)
        {
            var factories = c.Resolve<IEnumerable<IQueueWriterFactory>>();

            var queues = new List<IQueueWriter>(1);
            foreach (var factory in factories)
            {
                IQueueWriter writer;
                if (factory.TryGetWriteQueue(_endpoint, _queueName, out writer))
                {
                    queues.Add(writer);
                }
            }

            if (queues.Count == 0)
            {
                var message =
                    string.Format("There are no queues for the '{0}'. Did you forget to register a factory?",
                        _queueName);
                throw new InvalidOperationException(message);
            }
            if (queues.Count > 1)
            {
                string message = string.Format(
                    "There are multiple queues for name '{0}'. Have you registered duplicate factories?", _queueName);
                throw new InvalidOperationException(message);
            }

            var observer = c.Resolve<ISystemObserver>();

            return new DefaultMessageSender(queues[0], observer, _queueName, _keyGenerator);
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            componentRegistry.Register(BuildDefaultMessageSender);
        }
    }
}