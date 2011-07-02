#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core;
using System.Linq;

namespace Lokad.Cqrs.Feature.FilePartition
{
    public sealed class FilePartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly FileStorageConfig _fullPath;
        readonly string[] _fileQueues;

        
        readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

        Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher> _dispatcher;
        Func<IComponentContext, IEnvelopeQuarantine> _quarantineFactory;

        public FilePartitionModule WhereFilter(Action<MessageDirectoryFilter> filter)
        {
            filter(_filter);
            return this;
        }

        public FilePartitionModule(FileStorageConfig fullPath, string[] fileQueues)
        {
            _fullPath = fullPath;
            _fileQueues = fileQueues;


            DispatchAsEvents();

            Quarantine(c => new MemoryQuarantine());
        }

        public void DispatcherIs(Func<IComponentContext, MessageActivationInfo[], IMessageDispatchStrategy, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
        }


        public void Quarantine(Func<IComponentContext, IEnvelopeQuarantine> factory)
        {
            _quarantineFactory = factory;
        }

        public void DispatchAsEvents()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchOneEvent(map, ctx.Resolve<ISystemObserver>(), strategy));
        }

        public void DispatchAsCommandBatch()
        {
            DispatcherIs((ctx, map, strategy) => new DispatchCommandBatch(map, strategy));
        }
        public void DispatchToRoute(Func<ImmutableEnvelope, string> route)
        {
            DispatcherIs((ctx, map, strategy) => new DispatchMessagesToRoute(ctx.Resolve<QueueWriterRegistry>(), route));
        }

        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();
            var streamer = context.Resolve<IEnvelopeStreamer>();

            var builder = context.Resolve<MessageDirectoryBuilder>();

            var map = builder.BuildActivationMap(_filter.DoesPassFilter);
            var strategy = context.Resolve<IMessageDispatchStrategy>();
            var dispatcher = _dispatcher(context, map, strategy);
            dispatcher.Init();


            var queues = _fileQueues
                .Select(n => Path.Combine(_fullPath.Folder.FullName, n))
                .Select(f => new DirectoryInfo(f))
                .ToArray();
            var inbox = new FilePartitionInbox(queues, _fileQueues, streamer, u => TimeSpan.FromMilliseconds(200));
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