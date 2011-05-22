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
    public class CqrsEngineBuilder : HideObjectMembersFromIntelliSense
    {
        readonly SerializationContractRegistry _dataSerialization = new SerializationContractRegistry();
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);
        readonly MessageDirectoryModule _domain = new MessageDirectoryModule();
        readonly StorageModule _storage = new StorageModule();

        readonly QueueWriterRegistry _writerRegistry = new QueueWriterRegistry();

        public readonly List<IObserver<ISystemEvent>> Observers = new List<IObserver<ISystemEvent>>
            {
                new ImmediateTracingObserver()
            };
        

        public void RegisterDataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        public void RegisterEnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        readonly List<IModule> _moduleEnlistments = new List<IModule>();


        public void EnlistModule(IModule module)
        {
            _moduleEnlistments.Add(module);
        }

        
        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CqrsEngineBuilder Domain(Action<MessageDirectoryModule> config)
        {
            config(_domain);
            return this;
        }

        readonly ContainerBuilder _builder = new ContainerBuilder();

        public CqrsEngineBuilder Advanced(Action<ContainerBuilder> build)
        {
            build(_builder);
            return this;
        }
        
        public CqrsEngineBuilder Observer(IObserver<ISystemEvent> observer)
        {
            Observers.Add(observer);
            return this;
        }

        public CqrsEngineBuilder Memory(Action<MemoryModule> configure)
        {
            var m = new MemoryModule();
            configure(m);
            EnlistModule(m);
            return this;
        }

        public CqrsEngineBuilder Storage(Action<StorageModule> configure)
        {
            configure(_storage);
            return this;
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
            var system = new SystemObserver(Observers.ToArray());

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

            
            reg.Register(_writerRegistry);
            reg.Register(dataSerializer);
            reg.Register<IEnvelopeStreamer>(c => streamer);
            reg.Register(new MessageDuplicationManager());
            
        }
    }
}