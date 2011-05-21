#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Envelope;
using System.Linq;

namespace Lokad.Cqrs.Core.Serialization
{
    public sealed class SerializationModule : HideObjectMembersFromIntelliSense, IModule
    {
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);
        

        public void RegisterDataSerializer(Func<Type[],IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        public void RegisterEnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        public void Configure(IComponentRegistry container)
        {
            container.Register<IEnvelopeStreamer>(c => new EnvelopeStreamer(_envelopeSerializer, _dataSerializer(c.Resolve<IKnowSerializationTypes>().GetKnownTypes().ToArray())));
        }
    }
}