using System;

namespace Lokad.Cqrs.Feature.Consume.Events
{
	public sealed class FailedToConsumeMessage : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string EnvelopeId { get; private set; }
		public string QueueName { get; private set; }

		public FailedToConsumeMessage(Exception exception, string envelopeId, string queueName)
		{
			Exception = exception;
			EnvelopeId = envelopeId;
			QueueName = queueName;
		}
	}

	public sealed class FailedToAckMessage : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string EnvelopeId { get; private set; }
		public string QueueName { get; private set; }

		public FailedToAckMessage(Exception exception, string envelopeId, string queueName)
		{
			Exception = exception;
			EnvelopeId = envelopeId;
			QueueName = queueName;
		}
	}
}