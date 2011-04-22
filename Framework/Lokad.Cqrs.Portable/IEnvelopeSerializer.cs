using System.IO;
using Lokad.Cqrs.Envelope;

namespace Lokad.Cqrs
{
	public interface IEnvelopeSerializer
	{
		void SerializeEnvelope(Stream stream, EnvelopeContract contract);
		EnvelopeContract DeserializeEnvelope(Stream stream);
	}
}