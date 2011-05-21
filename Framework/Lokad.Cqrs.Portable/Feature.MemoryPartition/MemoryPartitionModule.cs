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
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionModule : HideObjectMembersFromIntelliSense, IModule
    {
        readonly string[] _memoryQueues;

        
        PartialRegistration<IEnvelopeQuarantine> _quarantinePartial;
        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        Func<IComponentContext, IMessageDispatchStrategy> _strategy;
        Func<IComponentContext, MessageActivationMap, IMessageDispatchStrategy, ISingleThreadMessageDispatcher> _dispatcher;

        public MemoryPartitionModule WhereFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
            return this;
        }

        public MemoryPartitionModule(string[] memoryQueues)
        {
            _memoryQueues = memoryQueues;

            DispatchStrategy(ctx => new AutofacDispatchStrategy(
                ctx.Resolve<ILifetimeScope>(),
                TransactionEvil.Factory(TransactionScopeOption.RequiresNew),
                ctx.Resolve<IMethodInvoker>()));

            DispatchAsEvents();

            QuarantineIs<MemoryQuarantine>();


        }


        public void DispatchStrategy(Func<IComponentContext, IMessageDispatchStrategy> factory)
        {
            _strategy = factory;
        }


        public void DispatcherIs(Func<IComponentContext, MessageActivationMap, IMessageDispatchStrategy, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
        }


        public void QuarantineIs<TQuarantine>(Action<TQuarantine> optionalConfig = null)
            where TQuarantine : class, IEnvelopeQuarantine
        {
            _quarantinePartial = PartialRegistration<IEnvelopeQuarantine>.From(optionalConfig);
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
            DispatcherIs((ctx, map, strategy) => new DispatchMessagesToRoute(ctx.Resolve<IEnumerable<IQueueWriterFactory>>(), route));
        }

        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();


            var builder = context.Resolve<MessageDirectoryBuilder>();

            var map = builder.BuildActivationMap(_filter.DoesPassFilter);

            var strategy = _strategy(context);
            var dispatcher = _dispatcher(context, map, strategy);
            dispatcher.Init();



            var factory = context.Resolve<MemoryPartitionFactory>();
            var notifier = factory.GetMemoryInbox(_memoryQueues);

            var quarantine = _quarantinePartial.ResolveWithTypedParams(context);
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine, manager);
            return transport;
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();
            _quarantinePartial.Register(builder);

            builder.Register(BuildConsumingProcess);
            builder.Update(componentRegistry);
        }
    }
}