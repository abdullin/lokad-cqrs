#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class ModuleForMemoryPartition : BuildSyntaxHelper, IModule
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
            Dispatch<DispatchEventToMultipleConsumers>(x => { });
        }

        public ModuleForMemoryPartition Dispatch<TDispatcher>(Action<TDispatcher> configure)
            where TDispatcher : class, ISingleThreadMessageDispatcher
        {
            _dispatcher = Tuple.Create(typeof (TDispatcher),
                new Action<ISingleThreadMessageDispatcher>(d => configure((TDispatcher) d)));
            return this;
        }


        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();


            var dir = context.Resolve<MessageDirectoryBuilder>();
            var directory = dir.BuildDirectory(_filter.DoesPassFilter);
            var typedParameter = TypedParameter.From(directory);
            var dispatcher = (ISingleThreadMessageDispatcher) context.Resolve(_dispatcher.Item1, typedParameter);
            _dispatcher.Item2(dispatcher);
            dispatcher.Init();

            var factory = context.Resolve<MemoryPartitionFactory>();
            var notifier = factory.GetMemoryInbox(_memoryQueues);

            var transport = new DispatcherProcess(log, dispatcher, notifier);
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