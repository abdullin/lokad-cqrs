using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Build.Client
{
    public class CqrsClientBuilder : HideObjectMembersFromIntelliSense
    {
        readonly List<IModule> _enlistments = new List<IModule>();

        public void EnlistModule(IModule module)
        {
            _enlistments.Add(module);
        }

        public bool DisableDefaultObserver { get; set; }

        public CqrsClientBuilder EnlistObserver(IObserver<ISystemEvent> observer)
        {
            _builder.RegisterInstance(observer);
            return this;
        }

        public CqrsClientBuilder EnlistObserver<TObserver>() where TObserver : IObserver<ISystemEvent>
        {
            _builder.RegisterType<TObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            return this;
        }

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

        readonly ContainerBuilder _builder = new ContainerBuilder();

        public CqrsClientBuilder Advanced(Action<ContainerBuilder> build)
        {
            build(_builder);
            return this;
        }

        public CqrsClientBuilder Storage(Action<StorageModule> configure)
        {
            configure(_storageModule);
            return this;
        }

        MessageDirectoryModule _domain = new MessageDirectoryModule();
        StorageModule _storageModule = new StorageModule();

        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CqrsClientBuilder Domain(Action<MessageDirectoryModule> config)
        {
            config(_domain);
            return this;
        }

        void InnerSystemRegisterObservations()
        {
            _builder.RegisterType<ReactiveSystemObserverAdapter>().SingleInstance().As<ISystemObserver>();
            if (!DisableDefaultObserver)
            {
                _builder.RegisterType<ImmediateTracingObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            }
        }

        readonly List<Type> _serializationList = new List<Type>();

        public CqrsClient Build()
        {
            AutoConfigure();

            var container = _builder.Build();

            var registry = container.ComponentRegistry;
            
            Configure(registry);
            return new CqrsClient(container);
        }

        void Configure(IComponentRegistry reg)
        {
            reg.Register<IEnvelopeStreamer>(c =>
                {
                    var serializer = _dataSerializer(_serializationList.ToArray());
                    return new EnvelopeStreamer(_envelopeSerializer, serializer);
                });
            _domain.Configure(reg, _serializationList);
            _storageModule.Configure(reg);
        }

        void AutoConfigure() 
        {
            InnerSystemRegisterObservations();
            

            foreach (var module in _enlistments)
            {
                _builder.RegisterModule(module);
            }
        }

     
    }
}