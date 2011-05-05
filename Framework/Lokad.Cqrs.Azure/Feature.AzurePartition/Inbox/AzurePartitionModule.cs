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
        TimeSpan _queueVisibilityTimeout = TimeSpan.FromSeconds(30);
        Tuple<Type, Action<ISingleThreadMessageDispatcher>> _dispatcher;

        readonly string _accountName;

        public AzurePartitionModule(string accountId, string[] queueNames)
        {
            _accountName = accountId;
            _queueNames = queueNames.ToSet();
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
        public void DispatchToRoute(Func<ImmutableEnvelope,string> route)
        {
            DispatcherIs<DispatchMessagesToRoute>(c => c.SpecifyRouter(route));
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

            var configurations = context.Resolve<IEnumerable<IAzureStorageConfiguration>>();

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
            var quarantine = new MemoryQuarantine();
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine, manager);
            return transport;
        }

        public AzurePartitionModule QueueVisibility(int timeoutMilliseconds)
        {
            _queueVisibilityTimeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            return this;
        }

        public AzurePartitionModule QueueVisibility(TimeSpan timespan)
        {
            _queueVisibilityTimeout = timespan;
            return this;
        }
    }
}