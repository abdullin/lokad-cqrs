#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using Lokad.Cqrs.Core;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class AzurePartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly HashSet<string> _queueNames = new HashSet<string>();
        TimeSpan _queueVisibilityTimeout;
        Func<IComponentContext, IEnvelopeQuarantine> _quarantineFactory;

        Func<uint, TimeSpan> _decayPolicy;

        readonly IAzureStorageConfig _config;


        Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher> _dispatcher;


        public AzurePartitionModule(IAzureStorageConfig config, string[] queueNames)
        {
            DispatchAsEvents();
            
            QueueVisibility(30000);



            _config = config;
            _queueNames = new HashSet<string>(queueNames);

            Quarantine(c => new MemoryQuarantine());
            DecayPolicy(TimeSpan.FromSeconds(2));
        }



        public void DispatcherIs(Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
        }

        /// <summary>
        /// Allows to specify custom <see cref="IEnvelopeQuarantine"/> optionally resolving 
        /// additional instances from the container
        /// </summary>
        /// <param name="factory">The factory method to specify custom <see cref="IEnvelopeQuarantine"/>.</param>
        public void Quarantine(Func<IComponentContext, IEnvelopeQuarantine> factory)
        {
            _quarantineFactory = factory;
        }

        /// <summary>
        /// Sets the custom decay policy used to throttle Azure queue checks, when there are no messages for some time.
        /// This overload eventually slows down requests till the max of <paramref name="maxInterval"/>
        /// </summary>
        /// <param name="maxInterval">The maximum interval to keep between checks, when there are no messages in the queue.</param>
        public void DecayPolicy(TimeSpan maxInterval)
        {
            //var seconds = (Rand.Next(0, 1000) / 10000d).Seconds();
            var seconds = maxInterval.TotalSeconds;
            _decayPolicy = l =>
                {
                    if (l >= 31)
                    {
                        return maxInterval;
                    }

                    if (l == 0)
                    {
                        l += 1;
                    }

                    var foo = Math.Pow(2, (l - 1)/5.0)/64d*seconds;

                    return TimeSpan.FromSeconds(foo);
                };
        }

        /// <summary>
        /// Sets the custom decay policy used to throttle Azure queue checks, when there are no messages for some time.
        /// </summary>
        /// <param name="decayPolicy">The decay policy, which is function that returns time to sleep after Nth empty check.</param>
        public void DecayPolicy(Func<uint ,TimeSpan> decayPolicy)
        {
            _decayPolicy = decayPolicy;
        }


        /// <summary>
        /// Wires <see cref="DispatchOneEvent"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching a single event to zero or more consumers.
        /// </summary>
        public void DispatchAsEvents()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchOneEvent(map, ctx.Resolve<ISystemObserver>(), strategy));
        }

        /// <summary>
        /// Wires <see cref="DispatchCommandBatch"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching multiple commands (in a single envelope) to one consumer each.
        /// </summary>
        public void DispatchAsCommandBatch()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchCommandBatch(map, strategy));
        }
        public void DispatchToRoute(Func<ImmutableEnvelope,string> route)
        {
            DispatcherIs((ctx, map, strategy) => new DispatchMessagesToRoute(ctx.Resolve<QueueWriterRegistry>(), route));
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

            var strategy = context.Resolve<IMessageDispatchStrategy>();
            var dispatcher = _dispatcher(context, map, strategy);
            dispatcher.Init();

            var streamer = context.Resolve<IEnvelopeStreamer>();

            var scheduling = context.Resolve<AzureSchedulingProcess>();
            var factory = new AzurePartitionFactory(streamer, log, _config, _queueVisibilityTimeout, scheduling, _decayPolicy);
            
            var notifier = factory.GetNotifier(_queueNames.ToArray());
            var quarantine = _quarantineFactory(context);
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine, manager);
            return transport;
        }

        /// <summary>
        /// Specifies queue visibility timeout for Azure Queues.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout milliseconds.</param>
        /// <returns></returns>
        public AzurePartitionModule QueueVisibility(int timeoutMilliseconds)
        {
            _queueVisibilityTimeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            return this;
        }

        /// <summary>
        /// Specifies queue visibility timeout for Azure Queues.
        /// </summary>
        /// <param name="timespan">The timespan.</param>
        /// <returns></returns>
        public AzurePartitionModule QueueVisibility(TimeSpan timespan)
        {
            _queueVisibilityTimeout = timespan;
            return this;
        }

        public void Configure(IComponentRegistry container)
        {
            container.Register(BuildConsumingProcess);

            if (!container.IsRegistered(new TypedService(typeof(AzureSchedulingProcess))))
            {
                var process = new AzureSchedulingProcess();
                container.Register(ctx => process);
                container.Register<IEngineProcess>(process);
            }
        }
    }
}