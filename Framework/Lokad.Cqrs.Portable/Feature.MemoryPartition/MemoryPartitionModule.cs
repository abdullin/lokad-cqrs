#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.DirectoryDispatch;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryPartitionModule : HideObjectMembersFromIntelliSense
    {
        readonly string[] _memoryQueues;

        Func<IComponentContext, ISingleThreadMessageDispatcher> _dispatcher;
        Func<IComponentContext, IEnvelopeQuarantine> _quarantineFactory;

        public MemoryPartitionModule(string[] memoryQueues)
        {
            _memoryQueues = memoryQueues;

            DispatchAsEvents();

            Quarantine(c => new MemoryQuarantine());
        }

        public void DispatcherIs(Func<IComponentContext, ISingleThreadMessageDispatcher> factory)
        {
            _dispatcher = factory;
        }

        /// <summary>
        /// Defines dispatcher as lambda method that is resolved against the container
        /// </summary>
        /// <param name="factory">The factory.</param>
        public void DispatcherIsLambda(Func<IComponentContext, Action<ImmutableEnvelope>> factory)
        {
            _dispatcher = context =>
                {
                    var lambda = factory(context);
                    return new ActionDispatcher(lambda);
                };
        }

        public void Quarantine(Func<IComponentContext, IEnvelopeQuarantine> factory)
        {
            _quarantineFactory = factory;
        }

        /// <summary>
        /// <para>Wires <see cref="DispatchOneEvent"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching a single event to zero or more consumers.</para>
        /// <para> Additional information is available in project docs.</para>
        /// </summary>
        public void DispatchAsEvents(Action<MessageDirectoryFilter> optionalFilter = null)
        {
            var action = optionalFilter ?? (x => { });
            _dispatcher = ctx => DirectoryDispatchFactory.OneEvent(ctx, action);
        }

        /// <summary>
        /// <para>Wires <see cref="DispatchCommandBatch"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching multiple commands (in a single envelope) to one consumer each.</para>
        /// <para> Additional information is available in project docs.</para>
        /// </summary>
        public void DispatchAsCommandBatch(Action<MessageDirectoryFilter> optionalFilter = null)
        {
            var action = optionalFilter ?? (x => { });
            _dispatcher = ctx => DirectoryDispatchFactory.CommandBatch(ctx, action);
        }

        public void DispatchToRoute(Func<ImmutableEnvelope, string> route)
        {
            DispatcherIs(ctx => new DispatchMessagesToRoute(ctx.Resolve<QueueWriterRegistry>(), route));
        }

        IEngineProcess BuildConsumingProcess(IComponentContext context)
        {
            var log = context.Resolve<ISystemObserver>();
            var dispatcher = _dispatcher(context);
            dispatcher.Init();

            var account = context.Resolve<MemoryAccount>();
            var factory = new MemoryPartitionFactory(account);
            var notifier = factory.GetMemoryInbox(_memoryQueues);

            var quarantine = _quarantineFactory(context);
            var manager = context.Resolve<MessageDuplicationManager>();
            var transport = new DispatcherProcess(log, dispatcher, notifier, quarantine, manager);
           
            return transport;
        }

        public void Configure(IComponentRegistry container)
        {
            container.Register(BuildConsumingProcess);
        }
    }
}