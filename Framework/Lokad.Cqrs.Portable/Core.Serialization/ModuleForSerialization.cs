#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Envelope;

namespace Lokad.Cqrs.Core.Serialization
{
    public sealed class ModuleForSerialization : BuildSyntaxHelper, IModule
    {
        readonly ContainerBuilder _builder = new ContainerBuilder();

        public void RegisterDataSerializer<TSerializer>() where TSerializer : IDataSerializer
        {
            _builder
                .RegisterType<TSerializer>()
                .As<IDataSerializer>()
                .SingleInstance();
        }

        public void RegisterEnvelopeSerializer<TEnvelopeSerializer>() where TEnvelopeSerializer : IEnvelopeSerializer
        {
            _builder
                .RegisterType<TEnvelopeSerializer>()
                .As<IEnvelopeSerializer>()
                .SingleInstance();
        }

        public void UseDataContractSerializer()
        {
            RegisterDataSerializer<DataSerializerWithDataContracts>();
            RegisterEnvelopeSerializer<EnvelopeSerializerWithDataContracts>();
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            _builder.RegisterType<EnvelopeStreamer>().As<IEnvelopeStreamer>()
                .SingleInstance();

            _builder.Update(componentRegistry);
        }
    }
}