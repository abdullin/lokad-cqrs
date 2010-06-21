using System.Collections.Generic;

namespace Lokad.Cqrs.Scheduled
{
	public interface IScheduledTaskBuilder
	{
		IEnumerable<ScheduledTaskInfo> BuildTasks();
	}
}