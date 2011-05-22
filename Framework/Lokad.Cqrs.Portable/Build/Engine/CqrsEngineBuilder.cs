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

        List<Type> _dataSerialization = new List<Type>();
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);

        

        public void RegisterDataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        public void RegisterEnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }




        readonly List<IModule> _moduleEnlistments = new List<IModule>();
        
        bool IsEnlisted<TModule>() where TModule : IModule
        {
            return _moduleEnlistments.Count(x => x is TModule) > 0;
        }

        


        public void EnlistModule(IModule module)
        {
            _moduleEnlistments.Add(module);
        }

        MessageDirectoryModule _domain = new MessageDirectoryModule();
        StorageModule _storage = new StorageModule();

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

        public CqrsEngineBuilder EnlistObserver(IObserver<ISystemEvent> observer)
        {
            _builder.RegisterInstance(observer);
            return this;
        }

        public CqrsEngineBuilder EnlistObserver<TObserver>() where TObserver : IObserver<ISystemEvent>
        {
            _builder.RegisterType<TObserver>()
                .As<IObserver<ISystemEvent>>()
                .As<TObserver>()
                .SingleInstance();
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

        public bool DisableDefaultObserver { get; set; }

        /// <summary>
        /// Builds this <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <returns>new instance of cloud engine host</returns>
        public CqrsEngineHost Build()
        {
            // nonconditional registrations
            // System presets
            InnerSystemRegisterObservations();

            

            _builder.RegisterType<DispatcherProcess>();
            _builder.RegisterType<MessageDuplicationManager>().SingleInstance();
            _builder.RegisterType<CqrsEngineHost>().SingleInstance();


                
            
            //if (_moduleEnlistments.Count(m => m is ))


            foreach (var module in _moduleEnlistments)
            {
                _builder.RegisterModule(module);
            }

            


            var container = _builder.Build();


            Configure(container.ComponentRegistry);
            

            var host = container.Resolve<CqrsEngineHost>(TypedParameter.From(container));
            host.Initialize();
            return host;
        }

        void Configure(IComponentRegistry registry)
        {

            registry.Register<IEnvelopeStreamer>(c =>
                {
                    var types = _dataSerialization.ToArray();
                    var dataSerializer = _dataSerializer(types);
                    return new EnvelopeStreamer(_envelopeSerializer, dataSerializer);
                });

            _domain.Configure(registry, _dataSerialization);
            _storage.Configure(registry);
        }

        void InnerSystemRegisterObservations()
        {
            _builder.RegisterType<ReactiveSystemObserverAdapter>().SingleInstance().As<ISystemObserver>();
            if (!DisableDefaultObserver)
            {
                _builder.RegisterType<ImmediateTracingObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            }
        }
    }
}