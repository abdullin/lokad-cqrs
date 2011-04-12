namespace Lokad.Cqrs.Core.Transport
{
	public interface IWriteQueueFactory
	{
		IWriteQueue GetWriteQueue(string queueName);
	}
}