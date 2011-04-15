namespace Lokad.Cqrs.Core.Transport
{
	public interface IWriteQueueFactory
	{
		IQueueWriter GetWriteQueue(string queueName);
	}
}