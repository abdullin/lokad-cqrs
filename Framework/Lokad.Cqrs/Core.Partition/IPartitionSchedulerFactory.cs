namespace Lokad.Cqrs.Core.Partition
{
	public interface IPartitionSchedulerFactory
	{
		IPartitionScheduler GetNotifier(string[] queueNames);
	}
}