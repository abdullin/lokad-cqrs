using System;

namespace Lokad.Cqrs.Feature.Recurrent.Events
{
	public sealed class FailedToExecuteScheduledTask : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string TaskName { get; private set; }

		public FailedToExecuteScheduledTask(Exception exception, string taskName)
		{
			Exception = exception;
			TaskName = taskName;
		}
	}
}