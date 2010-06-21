using System;

namespace Lokad.Cqrs.Scheduled
{
	public interface IScheduledTaskDispatcher
	{
		TimeSpan Execute(ScheduledTaskInfo info);
	}
}