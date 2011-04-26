#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build
{
    public static class ModuleExtensionForSerialization
    {
        public static void AutoDetectSerializer(this ModuleForSerialization @this)
        {
            @this.RegisterDataSerializer<DataSerializerWithAutoDetection>();
            @this.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
        }

        public static void UseProtoBufSerialization(this ModuleForSerialization self)
        {
            self.RegisterDataSerializer<DataSerializerWithProtoBuf>();
            self.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
        }
    }
}