using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Lokad.Cqrs.Envelope;

namespace Lokad.Cqrs.Core.Transport
{
	public sealed class EnvelopeSerializerWithDataContracts : IEnvelopeSerializer
	{
		readonly DataContractSerializer _serializer;

		public EnvelopeSerializerWithDataContracts()
		{
			_serializer = new DataContractSerializer(typeof(EnvelopeContract));
		}

		public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
		{
			//using (var compressed = destination.Compress(true))
			using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
			{
				_serializer.WriteObject(writer, contract);
			}
		}

		public EnvelopeContract DeserializeEnvelope(Stream stream)
		{
			using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
			{
				return (EnvelopeContract)_serializer.ReadObject(reader);
			}
		}
	}
}