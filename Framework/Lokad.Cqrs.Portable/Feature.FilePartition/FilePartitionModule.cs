#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.Dispatch.Directory;

namespace Lokad.Cqrs.Feature.FilePartition
{
    using LegacyDispatcherFactory = Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher>;

    public sealed class FilePartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly FileStorageConfig _fullPath;
        readonly string[] _fileQueues;
        Func<uint, TimeSpan> _decayPolicy;
        

        Func<IComponentContext, ISingleThreadMessageDispatcher> _dispatcher;

        Func<IComponentContext, IEnvelopeQuarantine> _quarantineFactory;

        /// <summary>
        /// Sets the custom decay policy used to throttle File checks, when there are no messages for some time.
        /// This overload eventually slows down requests till the max of <paramref name="maxInterval"/>.
        /// </summary>
        /// <param name="maxInterval">The maximum interval to keep between checks, when there are no messages in the queue.</param>
        public void DecayPolicy(TimeSpan maxInterval)
        {
            _decayPolicy = DecayEvil.BuildExponentialDecay(maxInterval);
        }

        /// <summary>
        /// Sets the custom decay policy used to throttle file queue checks, when there are no messages for some time.
        /// </summary>
        /// <param name="decayPolicy">The decay policy, which is function that returns time to sleep after Nth empty check.</param>
        public void DecayPolicy(Func<uint, TimeSpan> decayPolicy)
        {
            _decayPolicy = decayPolicy;
        }


        public FilePartitionModule(FileStorageConfig fullPath, string[] fileQueues)
        {
            _fullPath = fullPath;
            _fileQueues = fileQueues;


            DispatchAsEvents();

            Quarantine(c => new MemoryQuarantine());
            DecayPolicy(TimeSpan.FromMilliseconds(100));
        }

        void ResolveLegacyDispatcher(LegacyDispatcherFactory factory, Action<MessageDirectoryFilter> optionalFilter = null)
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

        public void DispatcherIs(Func<IComponentContext, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
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
            var streamer = context.Resolve<IEnvelopeStreamer>();
            
            var dispatcher = _dispatcher(context);
            dispatcher.Init();


            var queues = _fileQueues
                .Select(n => Path.Combine(_fullPath.Folder.FullName, n))
                .Select(n => new DirectoryInfo(n))
                .Select(f => new StatelessFileQueueReader(streamer, log, new Lazy<DirectoryInfo>(() =>
                    {
                        var poison = Path.Combine(f.FullName, "poison");
                        var di = new DirectoryInfo(poison);
                        di.Create();
                        return di;
                    }, LazyThreadSafetyMode.ExecutionAndPublication), f, f.Name))
                .ToArray();
            var inbox = new FilePartitionInbox(queues, _decayPolicy);
            var quarantine = _quarantineFactory(context);
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, inbox, quarantine, manager);


            return transport;
        }

        public void Configure(IComponentRegistry container)
        {
            container.Register(BuildConsumingProcess);
        }
    }
}