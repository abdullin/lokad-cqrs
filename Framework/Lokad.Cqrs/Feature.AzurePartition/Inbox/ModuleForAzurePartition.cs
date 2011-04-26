#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Evil;

// ReSharper disable MemberCanBePrivate.Global

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
    public sealed class ModuleForAzurePartition : Module
    {
        HashSet<string> _queueNames = new HashSet<string>();

        Tuple<Type, Action<ISingleThreadMessageDispatcher>> _dispatcher;

        public string ModuleName { get; private set; }

        Action<DispatcherProcess, IComponentContext> _applyToTransport = (transport, context) => { };

        public ModuleForAzurePartition(string[] queueNames)
        {
            _queueNames = queueNames.ToSet();
            ModuleName = "Handler-" + GetHashCode().ToString("X8");
            Dispatch<DispatchEventToMultipleConsumers>(x => { });
        }


        public ModuleForAzurePartition Dispatch<TDispatcher>(Action<TDispatcher> configure)
            where TDispatcher : class, ISingleThreadMessageDispatcher
        {
            _dispatcher = Tuple.Create(typeof (TDispatcher),
                new Action<ISingleThreadMessageDispatcher>(d => configure((TDispatcher) d)));
            return this;
        }

        public ModuleForAzurePartition ApplyToTransport(Action<DispatcherProcess, IComponentContext> config)
        {
            _applyToTransport += config;
            return this;
        }


        public ModuleForAzurePartition WhenMessageHandlerFails(Action<MessageEnvelope, Exception> handler)
        {
            throw new NotImplementedException();
            //return ApplyToTransport((transport, context) =>
            //    {
            //        transport.MessageHandlerFailed += handler;
            //        context.WhenDisposed(() => transport.MessageHandlerFailed -= handler);
            //    });
        }


        public ModuleForAzurePartition WhenMessageArrives(Action<MessageEnvelope> interceptor)
        {
            throw new NotImplementedException();
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType(_dispatcher.Item1);
            builder.Register(BuildConsumingProcess);
        }

        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        public ModuleForAzurePartition WhereFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
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

            var factory = context.Resolve<AzurePartitionFactory>();
            var notifier = factory.GetNotifier(_queueNames.ToArray());

            var quarantine = new MemoryQuarantine();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine);


            return transport;
        }
    }
}