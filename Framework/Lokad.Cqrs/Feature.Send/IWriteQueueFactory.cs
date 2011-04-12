namespace Lokad.Cqrs.Feature.Send
{
	public interface IWriteQueueFactory
	{
		IWriteQueue GetWriteQueue(string queueName);
	}
}