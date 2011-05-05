using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class EnvelopeSerializerWithProtoBuf : IEnvelopeSerializer
    {
        public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
        {
            Serializer.Serialize(stream, contract);
        }

        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            return Serializer.Deserialize<EnvelopeContract>(stream);
        }
    }
}