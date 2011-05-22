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
        readonly MessageDirectoryModule _domain = new MessageDirectoryModule();
        readonly StorageModule _storageModule = new StorageModule();

        readonly List<IModule> _enlistments = new List<IModule>();


        public readonly List<IObserver<ISystemEvent>> Observers = new List<IObserver<ISystemEvent>>()
            {
                new ImmediateTracingObserver()
            };


        public void EnlistModule(IModule module)
        {
            _enlistments.Add(module);
        }

        public CqrsClientBuilder Observer(IObserver<ISystemEvent> observer)
        {
            Observers.Add(observer);
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

        readonly List<Type> _serializationList = new List<Type>();

        public CqrsClient Build()
        {
            foreach (var module in _enlistments)
            {
                _builder.RegisterModule(module);
            }

            var container = _builder.Build();
            var system = new SystemObserver(Observers.ToArray());
            Configure(container.ComponentRegistry, system);
            return new CqrsClient(container);
        }

        void Configure(IComponentRegistry reg, ISystemObserver observer)
        {
            reg.Register(observer);
            reg.Register<IEnvelopeStreamer>(c =>
                {
                    var serializer = _dataSerializer(_serializationList.ToArray());
                    return new EnvelopeStreamer(_envelopeSerializer, serializer);
                });
            _domain.Configure(reg, _serializationList);
            _storageModule.Configure(reg);
        }
    }
}