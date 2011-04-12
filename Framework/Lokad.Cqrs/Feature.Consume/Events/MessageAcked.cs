namespace Lokad.Cqrs.Feature.Consume.Events
{
	public sealed class MessageAcked : ISystemEvent
	{
		public string QueueName { get; private set; }
		public string EnvelopeId { get; private set; }

		public MessageAcked(string queueName, string envelopeId)
		{
			QueueName = queueName;
			EnvelopeId = envelopeId;
		}
	}
}