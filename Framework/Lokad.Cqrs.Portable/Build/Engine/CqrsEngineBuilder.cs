#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Core;

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Fluent API for creating and configuring <see cref="CqrsEngineHost"/>
    /// </summary>
    public class CqrsEngineBuilder : HideObjectMembersFromIntelliSense, IAdvancedEngineBuilder
    {
        readonly SerializationContractRegistry _dataSerialization = new SerializationContractRegistry();
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);
        readonly MessageDirectoryModule _domain = new MessageDirectoryModule();
        readonly StorageModule _storage = new StorageModule();


        readonly IList<Func<IComponentContext, IQueueWriterFactory>> _activators = new List<Func<IComponentContext, IQueueWriterFactory>>();

        readonly List<IObserver<ISystemEvent>> _observers = new List<IObserver<ISystemEvent>>
            {
                new ImmediateTracingObserver()
            };


        void IAdvancedEngineBuilder.CustomDataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        void IAdvancedEngineBuilder.CustomEnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        void IAdvancedEngineBuilder.RegisterQueueWriterFactory(Func<IComponentContext,IQueueWriterFactory> activator)
        {
            _activators.Add(activator);
        }

        readonly List<IModule> _moduleEnlistments = new List<IModule>();


        void IAdvancedEngineBuilder.RegisterModule(IModule module)
        {
            _moduleEnlistments.Add(module);
        }

        
        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public void Domain(Action<MessageDirectoryModule> config)
        {
            config(_domain);
        }

        readonly ContainerBuilder _builder = new ContainerBuilder();

        void IAdvancedEngineBuilder.ConfigureContainer(Action<ContainerBuilder> build)
        {
            build(_builder);
        }

        void IAdvancedEngineBuilder.RegisterObserver(IObserver<ISystemEvent> observer)
        {
            _observers.Add(observer);
        }

        IList<IObserver<ISystemEvent>> IAdvancedEngineBuilder.Observers
        {
            get { return _observers; }
        }


        public void Memory(Action<MemoryModule> configure)
        {
            var m = new MemoryModule();
            configure(m);
            Advanced.RegisterModule(m);
        }

        public void Storage(Action<StorageModule> configure)
        {
            configure(_storage);
        }


        /// <summary>
        /// Builds this <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <returns>new instance of cloud engine host</returns>
        public CqrsEngineHost Build()
        {
            // nonconditional registrations
            // System presets
            _builder.RegisterType<DispatcherProcess>();
            
            
            
            foreach (var module in _moduleEnlistments)
            {
                _builder.RegisterModule(module);
            }
            var container = _builder.Build();
            var system = new SystemObserver(_observers.ToArray());

            var reg = container.ComponentRegistry;
            
            
            Configure(reg, system);

            var processes = container.Resolve<IEnumerable<IEngineProcess>>();
            var scope = container.Resolve<ILifetimeScope>();
            var host = new CqrsEngineHost(scope, system, processes);
            host.Initialize();
            return host;
        }

        void Configure(IComponentRegistry reg, ISystemObserver system) 
        {
            reg.Register(system);
            
            // domain should go before serialization
            _domain.Configure(reg, _dataSerialization);
            _storage.Configure(reg);

            var types = _dataSerialization.GetAndMakeReadOnly();
            var dataSerializer = _dataSerializer(types);
            var streamer = new EnvelopeStreamer(_envelopeSerializer, dataSerializer);

            
            reg.Register(BuildRegistry);
            reg.Register(dataSerializer);
            reg.Register<IEnvelopeStreamer>(c => streamer);
            reg.Register(new MessageDuplicationManager());
            
        }

        QueueWriterRegistry BuildRegistry(IComponentContext c) {
            var r = new QueueWriterRegistry();
                    
            foreach (var activator in _activators)
            {
                var factory = activator(c);
                r.Add(factory);
            }
            return r;
        }

        public IAdvancedEngineBuilder Advanced
        {
            get { return this; }
        }
    }
}