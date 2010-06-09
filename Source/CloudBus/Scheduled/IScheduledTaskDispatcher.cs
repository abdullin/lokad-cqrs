using System;

namespace CloudBus.Scheduled
{
	public interface IScheduledTaskDispatcher
	{
		TimeSpan Execute(ScheduledTaskInfo info);
	}
}