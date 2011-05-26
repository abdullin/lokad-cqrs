#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Build.Client;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build
{
    public static class ExtendSerializationModule
    {
        public static void AutoDetectSerializer(this CqrsClientBuilder self)
        {
            self.Advanced.DataSerializer(t => new DataSerializerWithAutoDetection(t));
            self.Advanced.EnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());
        }

        public static void UseProtoBufSerialization(this CqrsClientBuilder self)
        {
            self.Advanced.DataSerializer(t => new DataSerializerWithProtoBuf(t));
            self.Advanced.EnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());
        }
        public static void AutoDetectSerializer(this CqrsEngineBuilder self)
        {
            self.Advanced.CustomDataSerializer(t => new DataSerializerWithAutoDetection(t));
            self.Advanced.CustomEnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());
        }

        public static void UseProtoBufSerialization(this CqrsEngineBuilder self)
        {
            self.Advanced.CustomDataSerializer(t => new DataSerializerWithProtoBuf(t));
            self.Advanced.CustomEnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());
        }
    }
}