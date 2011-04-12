namespace Lokad.Cqrs.Feature.Recurrent.Events
{
	public sealed class ScheduledProcessInitialized : ISystemEvent
	{
		public int TaskCount { get; private set; }

		public ScheduledProcessInitialized(int taskCount)
		{
			TaskCount = taskCount;
		}
	}
}