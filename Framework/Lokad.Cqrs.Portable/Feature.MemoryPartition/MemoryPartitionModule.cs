#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.Dispatch.Directory;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    using LegacyDispatchFactory = Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher>;

    public sealed class MemoryPartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly string[] _memoryQueues;

        Func<IComponentContext, ISingleThreadMessageDispatcher> _dispatcher;
        Func<IComponentContext, IEnvelopeQuarantine> _quarantineFactory;

        public MemoryPartitionModule(string[] memoryQueues)
        {
            _memoryQueues = memoryQueues;

            DispatchAsEvents();

            Quarantine(c => new MemoryQuarantine());
        }

        public void DispatcherIs(Func<IComponentContext, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
        }
        
        void ResolveLegacyDispatcher(LegacyDispatchFactory factory, Action<MessageDirectoryFilter> optionalFilter = null)
        {
            _dispatcher = ctx =>
                {
                    var builder = ctx.Resolve<MessageDirectoryBuilder>();
                    var filter = new MessageDirectoryFilter();
                    if (null != optionalFilter)
                    {
                        optionalFilter(filter);
                    }
                    var map = builder.BuildActivationMap(filter.DoesPassFilter);

                    var strategy = ctx.Resolve<IMessageDispatchStrategy>();
                    return factory(ctx, map, strategy);
                };
        }

        public void Quarantine(Func<IComponentContext, IEnvelopeQuarantine> factory)
        {
            _quarantineFactory = factory;
        }

        public void DispatchAsEvents(Action<MessageDirectoryFilter> optionalFilter = null)
        {
            ResolveLegacyDispatcher((ctx, map, strategy) => new DispatchOneEvent(map, ctx.Resolve<ISystemObserver>(), strategy), optionalFilter);
        }

        public void DispatchAsCommandBatch(Action<MessageDirectoryFilter> optionalFilter = null)
        {
            ResolveLegacyDispatcher((ctx, map, strategy) => new DispatchCommandBatch(map, strategy), optionalFilter);
        }
        public void DispatchToRoute(Func<ImmutableEnvelope, string> route)
        {
            DispatcherIs(ctx => new DispatchMessagesToRoute(ctx.Resolve<QueueWriterRegistry>(), route));
        }

        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();
            var dispatcher = _dispatcher(context);
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