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
    public sealed class ModuleForMemoryPartition : HideObjectMembersFromIntelliSense, IModule
    {
        readonly string[] _memoryQueues;
        Tuple<Type, Action<ISingleThreadMessageDispatcher>> _dispatcher;
        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        public ModuleForMemoryPartition WhereFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
            return this;
        }


        public ModuleForMemoryPartition(string[] memoryQueues)
        {
            _memoryQueues = memoryQueues;
            DispatchAsEvents();
        }

        public void DispatcherIs<TDispatcher>(Action<TDispatcher> optionalConfig = null)
            where TDispatcher : class, ISingleThreadMessageDispatcher
        {
            var config = optionalConfig ?? (dispatcher => { });
            var action = new Action<ISingleThreadMessageDispatcher>(d => config((TDispatcher) d));
            _dispatcher = Tuple.Create(typeof (TDispatcher),
                action);
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

            var directory = TypedParameter.From(activations);

            var dispatcher = (ISingleThreadMessageDispatcher) context.Resolve(_dispatcher.Item1, directory);
            _dispatcher.Item2(dispatcher);
            dispatcher.Init();

            var factory = context.Resolve<MemoryPartitionFactory>();
            var notifier = factory.GetMemoryInbox(_memoryQueues);

            var quarantine = new MemoryQuarantine();
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine, manager);
            return transport;
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(_dispatcher.Item1);
            builder.Register(BuildConsumingProcess);
            builder.Update(componentRegistry);
        }
    }
}