using NUnit.Framework;

namespace Lokad.Cqrs.Core.Envelope
{
    [TestFixture]
    public sealed class Play_all_for_Memory : When_envelope_is_serialized
    {
        protected override ImmutableMessageEnvelope RoundtripViaSerializer(MessageEnvelopeBuilder builder)
        {
            return builder.Build();
        }
    }
}