using System.Collections.Generic;

namespace CloudBus.Scheduled
{
	public interface IScheduledTaskBuilder
	{
		IEnumerable<ScheduledTaskInfo> BuildTasks();
	}
}