using System;

namespace Lokad.Cqrs.Feature.AzurePartition.Events
{
	public sealed class FailedToDeserializeMessage : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string QueueName { get; private set; }
		public string MessageId { get; private set; }

		public FailedToDeserializeMessage(Exception exception, string queueName, string messageId)
		{
			Exception = exception;
			QueueName = queueName;
			MessageId = messageId;
		}
	}
}