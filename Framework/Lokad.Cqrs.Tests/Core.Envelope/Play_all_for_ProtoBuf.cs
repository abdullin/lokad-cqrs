#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Core.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Envelope
{
    [TestFixture]
    public sealed class Play_all_for_ProtoBuf : When_envelope_is_serialized
    {
        readonly IEnvelopeStreamer _streamer = BuildStreamer(new EnvelopeSerializerWithProtoBuf());

        protected override MessageEnvelope RoundtripViaSerializer(MessageEnvelopeBuilder builder)
        {
            var bytes = _streamer.SaveDataMessage(builder.Build());
            return _streamer.ReadDataMessage(bytes);
        }
    }
}