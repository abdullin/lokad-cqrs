using Lokad.Cqrs.Core.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Envelope
{
    [TestFixture]
    public sealed class Play_all_for_DataContracts : When_envelope_is_serialized
    {
        readonly IEnvelopeStreamer _streamer = BuildStreamer(new EnvelopeSerializerWithProtoBuf());
        protected override MessageEnvelope RoundtripViaSerializer(MessageEnvelopeBuilder builder)
        {
            var bytes = _streamer.SaveDataMessage(builder.Build());
            return _streamer.ReadDataMessage(bytes);
        }
    }
}