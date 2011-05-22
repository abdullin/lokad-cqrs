#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class SendMessageModule : HideObjectMembersFromIntelliSense
    {
        readonly Func<IComponentContext, string,IQueueWriterFactory> _construct;
        readonly string _endpoint;
        readonly string _queueName;
        Func<string> _keyGenerator = () => Guid.NewGuid().ToString().ToLowerInvariant();


        public SendMessageModule(Func<IComponentContext, string, IQueueWriterFactory> construct, string endpoint, string queueName)
        {
            _construct = construct;
            _endpoint = endpoint;
            _queueName = queueName;
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
            var observer = c.Resolve<ISystemObserver>();
            var registry = c.Resolve<QueueWriterRegistry>();
            var factory = registry.GetOrAdd(_endpoint, s => _construct(c,s));
            var queue = factory.GetWriteQueue(_queueName);
            return new DefaultMessageSender(queue, observer, _keyGenerator);
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            componentRegistry.Register(BuildDefaultMessageSender);
        }
    }
}