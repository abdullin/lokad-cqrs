#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build
{
    public static class ExtendSerializationModule
    {
        public static void AutoDetectSerializer(this SerializationModule @this)
        {
            @this.RegisterDataSerializer<DataSerializerWithAutoDetection>();
            @this.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
        }

        public static void UseProtoBufSerialization(this SerializationModule self)
        {
            self.RegisterDataSerializer<DataSerializerWithProtoBuf>();
            self.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
        }
    }
}