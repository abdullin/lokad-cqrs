namespace Lokad.Cqrs.Core.Partition
{
	public interface IPartitionInboxFactory
	{
		IPartitionInbox GetNotifier(string[] queueNames);
	}
}