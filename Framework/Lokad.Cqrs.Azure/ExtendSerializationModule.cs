#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Build.Client;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs
{
    public static class ExtendSerializationModule
    {
        public static void UseProtoBufSerialization(this CqrsClientBuilder self, ICollection<Type> knownTypes)
        {
            self.Advanced.DataSerializer(new DataSerializerWithProtoBuf(knownTypes));
            self.Advanced.EnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());
        }

        public static void UseProtoBufSerialization(this CqrsEngineBuilder self, ICollection<Type> knownTypes)
        {
            self.Advanced.CustomDataSerializer(new DataSerializerWithProtoBuf(knownTypes));
            self.Advanced.CustomEnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());
        }
    }
}