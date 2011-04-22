namespace Lokad.Cqrs.Core.Inbox
{
	public interface IPartitionInboxFactory
	{
		IPartitionInbox GetNotifier(string[] queueNames);
	}
}