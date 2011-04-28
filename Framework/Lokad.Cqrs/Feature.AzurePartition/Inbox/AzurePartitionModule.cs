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
    public sealed class AzurePartitionModule : Module
    {
        readonly HashSet<string> _queueNames = new HashSet<string>();
        TimeSpan _queueVisibilityTimeout = 30.Seconds();
        Tuple<Type, Action<ISingleThreadMessageDispatcher>> _dispatcher;

        Action<AzurePartitionFactory> _onActivateFactory = factory => { };

        string _accountName;

        public AzurePartitionModule(string accountId, string[] queueNames)
        {
            _accountName = accountId;
            _queueNames = queueNames.ToSet();
            Dispatch<DispatchEventToMultipleConsumers>(x => { });
        }


        public AzurePartitionModule Dispatch<TDispatcher>(Action<TDispatcher> configure)
            where TDispatcher : class, ISingleThreadMessageDispatcher
        {
            _dispatcher = Tuple.Create(typeof (TDispatcher),
                new Action<ISingleThreadMessageDispatcher>(d => configure((TDispatcher) d)));
            return this;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType(_dispatcher.Item1);
            builder.Register(BuildConsumingProcess);
            builder.RegisterType<AzureSchedulingProcess>().As<IEngineProcess, AzureSchedulingProcess>();
        }

        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        public AzurePartitionModule DirectoryFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
            return this;
        }

        public AzurePartitionModule NonDefaultAccount(string accountName)
        {
            _accountName = accountName;
            return this;
        }


        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();


            var builder = context.Resolve<MessageDirectoryBuilder>();

            var map = builder.BuildActivationMap(_filter.DoesPassFilter);
            var directory = TypedParameter.From(map);

            var dispatcher = (ISingleThreadMessageDispatcher) context.Resolve(_dispatcher.Item1, directory);
            _dispatcher.Item2(dispatcher);
            dispatcher.Init();

            var streamer = context.Resolve<IEnvelopeStreamer>();

            var configurations = context.Resolve<IEnumerable<IAzureClientConfiguration>>();

            var configuration = configurations.FirstOrDefault(c => c.AccountName == _accountName);
            if (configuration == null)
            {
                var message = string.Format("Failed to locate Azure account '{0}'. Have you registered it?",
                    _accountName);
                throw new InvalidOperationException(message);
            }

            var scheduling = context.Resolve<AzureSchedulingProcess>();
            var factory = new AzurePartitionFactory(streamer, log, configuration, _queueVisibilityTimeout, scheduling);
            
            var notifier = factory.GetNotifier(_queueNames.ToArray());
            _onActivateFactory(factory);
            var quarantine = new MemoryQuarantine();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine);
            return transport;
        }

        public AzurePartitionModule QueueVisibilityTimeout(int timeoutMilliseconds)
        {
            _queueVisibilityTimeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            return this;
        }

        public AzurePartitionModule WhenFactoryCreated(Action<AzurePartitionFactory> config)
        {
            _onActivateFactory += config;
            return this;
        }

        public AzurePartitionModule QueueVisibilityTimeout(TimeSpan timespan)
        {
            _queueVisibilityTimeout = timespan;
            return this;
        }
    }
}