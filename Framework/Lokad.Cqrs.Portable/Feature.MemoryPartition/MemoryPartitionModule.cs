#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionModule : HideObjectMembersFromIntelliSense, IModule
    {
        readonly string[] _memoryQueues;

        PartialRegistration<ISingleThreadMessageDispatcher> _dispatcherPartial;
        PartialRegistration<IEnvelopeQuarantine> _quarantinePartial;

        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        public MemoryPartitionModule WhereFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
            return this;
        }

        public MemoryPartitionModule(string[] memoryQueues)
        {
            _memoryQueues = memoryQueues;
            DispatchAsEvents();
            QuarantineIs<MemoryQuarantine>();
        }

        public void DispatcherIs<TDispatcher>(Action<TDispatcher> optionalConfig = null)
            where TDispatcher : class, ISingleThreadMessageDispatcher
        {
            _dispatcherPartial =  PartialRegistration<ISingleThreadMessageDispatcher>.From(optionalConfig);
        }

        public void QuarantineIs<TQuarantine>(Action<TQuarantine> optionalConfig = null)
            where TQuarantine : class, IEnvelopeQuarantine
        {
            _quarantinePartial = PartialRegistration<IEnvelopeQuarantine>.From(optionalConfig);
        }

        public void DispatchAsEvents()
        {
            DispatcherIs<DispatchOneEvent>();
        }

        public void DispatchAsCommandBatch(Action<DispatchCommandBatch> optionalConfig = null)
        {
            DispatcherIs(optionalConfig);
        }

        public void DispatchToRoute(Func<ImmutableEnvelope, string> route)
        {
            DispatcherIs<DispatchMessagesToRoute>(c => c.SpecifyRouter(route));
        }

        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();


            var builder = context.Resolve<MessageDirectoryBuilder>();

            var activations = builder.BuildActivationMap(_filter.DoesPassFilter);

            var dispatcher = _dispatcherPartial.ResolveWithTypedParams(context, activations);
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

            _dispatcherPartial.Register(builder);
            _quarantinePartial.Register(builder);

            builder.Register(BuildConsumingProcess);
            builder.Update(componentRegistry);
        }
    }
}