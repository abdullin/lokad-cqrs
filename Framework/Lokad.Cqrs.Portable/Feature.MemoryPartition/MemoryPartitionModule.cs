#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Transactions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly string[] _memoryQueues;

        
        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher> _dispatcher;
        Func<IComponentContext, IEnvelopeQuarantine> _quarantineFactory;

        public MemoryPartitionModule WhereFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
            return this;
        }

        public MemoryPartitionModule(string[] memoryQueues)
        {
            _memoryQueues = memoryQueues;


            DispatchAsEvents();

            Quarantine(c => new MemoryQuarantine());
        }

        public void DispatcherIs(Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
        }


        public void Quarantine(Func<IComponentContext, IEnvelopeQuarantine> factory)
        {
            _quarantineFactory = factory;
        }

        public void DispatchAsEvents()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchOneEvent(map, ctx.Resolve<ISystemObserver>(), strategy));
        }

        public void DispatchAsCommandBatch()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchCommandBatch(map, strategy));
        }
        public void DispatchToRoute(Func<ImmutableEnvelope, string> route)
        {
            DispatcherIs((ctx, map, strategy) => new DispatchMessagesToRoute(ctx.Resolve<QueueWriterRegistry>(), route));
        }

        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();


            var builder = context.Resolve<MessageDirectoryBuilder>();

            var map = builder.BuildActivationMap(_filter.DoesPassFilter);
            var strategy = context.Resolve<IMessageDispatchStrategy>();
            var dispatcher = _dispatcher(context, map, strategy);
            dispatcher.Init();

            var account = context.Resolve<MemoryAccount>();
            var factory = new MemoryPartitionFactory(account);
            var notifier = factory.GetMemoryInbox(_memoryQueues);

            var quarantine = _quarantineFactory(context);
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine, manager);

           
            return transport;
        }

        public void Configure(IComponentRegistry container)
        {
            if (!container.IsRegistered(new TypedService(typeof(MemoryAccount))))
            {
                var account = new MemoryAccount();
                container.Register(account);
            }
            container.Register(BuildConsumingProcess);
        }
    }
}