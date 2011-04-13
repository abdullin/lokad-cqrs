using System;

namespace Lokad.Cqrs.Feature.AzureConsumer.Events
{
	public sealed class FailedToReadMessage : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string QueueName { get; private set; }

		public FailedToReadMessage(Exception exception, string queueName)
		{
			Exception = exception;
			QueueName = queueName;
		}
	}
}