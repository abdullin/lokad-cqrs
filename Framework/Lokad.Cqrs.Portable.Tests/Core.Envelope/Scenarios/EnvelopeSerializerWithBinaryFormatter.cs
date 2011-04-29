using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lokad.Cqrs.Core.Envelope.Scenarios
{
    class EnvelopeSerializerWithBinaryFormatter : IEnvelopeSerializer
    {
        public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
        {
            Formatter.Serialize(stream, contract);
        }


        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            return (EnvelopeContract)Formatter.Deserialize(stream);
        }

        static readonly BinaryFormatter Formatter = new BinaryFormatter();


        
    }
}