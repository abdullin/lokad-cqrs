#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;

// ReSharper disable MemberCanBePrivate.Global

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class AzurePartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly HashSet<string> _queueNames = new HashSet<string>();
        TimeSpan _queueVisibilityTimeout = TimeSpan.FromSeconds(30);
        PartialRegistration<IEnvelopeQuarantine> _quarantinePartial;

        Func<uint, TimeSpan> _decayPolicy;

        readonly string _accountName;

        Func<TransactionScope> _transactionScope;
        Func<IComponentContext, Func<TransactionScope>, IMessageDispatchStrategy> _strategy;
        Func<IComponentContext, MessageActivationMap, IMessageDispatchStrategy, ISingleThreadMessageDispatcher> _dispatcher;

        public void Transactional(Func<TransactionScope> factory)
        {
            _transactionScope = factory;
        }

        public AzurePartitionModule(string accountId, string[] queueNames)
        {

            _transactionScope = TransactionEvil.Transactional(TransactionScopeOption.RequiresNew);
            _strategy = (context, tx) => new AutofacDispatchStrategy(
                context.Resolve<ILifetimeScope>(),
                tx,
                context.Resolve<IMethodInvoker>());

            _dispatcher = (ctx, map, strategy) => new DispatchOneEvent(map, ctx.Resolve<ISystemObserver>(), strategy); 




            _accountName = accountId;
            _queueNames = new HashSet<string>(queueNames);
            
            QuarantineIs<MemoryQuarantine>();
            DecayPolicy(TimeSpan.FromSeconds(2));
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

        public void DecayPolicy(TimeSpan timeout)
        {
            //var seconds = (Rand.Next(0, 1000) / 10000d).Seconds();
            var seconds = timeout.TotalSeconds;
            _decayPolicy = l =>
                {
                    if (l >= 31)
                    {
                        return timeout;
                    }

                    if (l == 0)
                    {
                        l += 1;
                    }

                    var foo = Math.Pow(2, (l - 1)/5.0)/64d*seconds;

                    return TimeSpan.FromSeconds(foo);
                };
        }


        public void DispatchAsEvents()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchOneEvent(map, ctx.Resolve<ISystemObserver>(), strategy));
        }
        
        public void DispatchAsCommandBatch()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchCommandBatch(map, strategy));
        }
        public void DispatchToRoute(Func<ImmutableEnvelope,string> route)
        {
            DispatcherIs((ctx, map, strategy) => new DispatchMessagesToRoute(ctx.Resolve<IEnumerable<IQueueWriterFactory>>(), route));
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

            var strategy = _strategy(context, _transactionScope);
            var dispatcher = _dispatcher(context, map, strategy);
            dispatcher.Init();

            var streamer = context.Resolve<IEnvelopeStreamer>();

            var configurations = context.Resolve<AzureStorageRegistry>();

            IAzureStorageConfiguration config;
            if (!configurations.TryGet(_accountName, out config))
            {
                var message = string.Format("Failed to locate Azure account '{0}'. Have you registered it?",
                    _accountName);
                throw new InvalidOperationException(message);
            }

            var scheduling = context.Resolve<AzureSchedulingProcess>();
            var factory = new AzurePartitionFactory(streamer, log, config, _queueVisibilityTimeout, scheduling, _decayPolicy);
            
            var notifier = factory.GetNotifier(_queueNames.ToArray());
            var quarantine = _quarantinePartial.ResolveWithTypedParams(context);
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

        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();
            _quarantinePartial.Register(builder);
            builder.Register(BuildConsumingProcess);
            builder.RegisterType<AzureSchedulingProcess>().As<IEngineProcess, AzureSchedulingProcess>();
            builder.Update(componentRegistry);
        }
    }
}