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
        readonly List<Type> _dataSerialization = new List<Type>();
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);
        readonly MessageDirectoryModule _domain = new MessageDirectoryModule();
        readonly StorageModule _storage = new StorageModule();


        public readonly List<IObserver<ISystemEvent>> Observers = new List<IObserver<ISystemEvent>>()
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
            _builder.RegisterModule(m);
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
            _builder.RegisterType<MessageDuplicationManager>().SingleInstance();
            
            foreach (var module in _moduleEnlistments)
            {
                _builder.RegisterModule(module);
            }
            var container = _builder.Build();

            var system = new SystemObserver(Observers.ToArray());
            Configure(container.ComponentRegistry, system);

            var processes = container.Resolve<IEnumerable<IEngineProcess>>();
            var scope = container.Resolve<ILifetimeScope>();
            var host = new CqrsEngineHost(scope, system, processes);
            host.Initialize();
            return host;
        }

        void Configure(IComponentRegistry registry, ISystemObserver observer)
        {
            registry.Register(observer);
            registry.Register<IEnvelopeStreamer>(c =>
                {
                    var types = _dataSerialization.ToArray();
                    var dataSerializer = _dataSerializer(types);
                    return new EnvelopeStreamer(_envelopeSerializer, dataSerializer);
                });

            _domain.Configure(registry, _dataSerialization);
            _storage.Configure(registry);
        }
    }
}