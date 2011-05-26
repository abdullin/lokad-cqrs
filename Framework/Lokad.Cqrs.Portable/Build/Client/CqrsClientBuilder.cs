#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build.Client
{
    public class CqrsClientBuilder : HideObjectMembersFromIntelliSense, IAdvancedClientBuilder
    {
        readonly MessageDirectoryModule _domain = new MessageDirectoryModule();
        readonly StorageModule _storageModule = new StorageModule();

        readonly List<IModule> _enlistments = new List<IModule>();

        readonly QueueWriterRegistry _registry = new QueueWriterRegistry();


        readonly List<IObserver<ISystemEvent>> _observers = new List<IObserver<ISystemEvent>>()
            {
                new ImmediateTracingObserver()
            };

        IList<IObserver<ISystemEvent>> IAdvancedClientBuilder.Observers
        {
            get { return _observers; }
        }

        void IAdvancedClientBuilder.RegisterModule(IModule module)
        {
            _enlistments.Add(module);
        }

        CqrsClientBuilder IAdvancedClientBuilder.RegisterObserver(IObserver<ISystemEvent> observer)
        {
            _observers.Add(observer);
            return this;
        }


        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);


        void IAdvancedClientBuilder.DataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        void IAdvancedClientBuilder.EnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        readonly ContainerBuilder _builder = new ContainerBuilder();

        public CqrsClientBuilder ConfigureContainer(Action<ContainerBuilder> build)
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

        readonly SerializationContractRegistry _serializationList = new SerializationContractRegistry();

        public CqrsClient Build()
        {
            foreach (var module in _enlistments)
            {
                _builder.RegisterModule(module);
            }

            var container = _builder.Build();
            var system = new SystemObserver(_observers.ToArray());
            Configure(container.ComponentRegistry, system);
            return new CqrsClient(container);
        }

        void Configure(IComponentRegistry reg, ISystemObserver observer)
        {
            reg.Register(observer);
            // domain should go before serialization
            _domain.Configure(reg, _serializationList);
            _storageModule.Configure(reg);

            var serializer = _dataSerializer(_serializationList.GetAndMakeReadOnly());
            var streamer = new EnvelopeStreamer(_envelopeSerializer, serializer);


            reg.Register(serializer);
            reg.Register<IEnvelopeStreamer>(c => streamer);
            reg.Register(_registry);
        }

        public IAdvancedClientBuilder Advanced
        {
            get { return this; }
        }
    }
}