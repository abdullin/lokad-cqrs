namespace Lokad.Cqrs.Core.Outbox
{
	public sealed class EnvelopeSent : ISystemEvent
	{
		public readonly string QueueName;
		public readonly string EnvelopeId;
		public readonly bool Transactional;
		public readonly string[] MappedTypes;

		public EnvelopeSent(string queueName, string envelopeId, bool transactional, string[] mappedTypes)
		{
			QueueName = queueName;
			EnvelopeId = envelopeId;
			Transactional = transactional;
			MappedTypes = mappedTypes;
		}
		public override string ToString()
		{
			return string.Format("Message {0} sent to {1} ({2})", EnvelopeId, QueueName, string.Join(",", MappedTypes));
		}
	}
}