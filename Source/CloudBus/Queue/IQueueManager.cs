namespace Bus2.Queue
{
	public interface IQueueManager
	{
		IReadMessageQueue GetReadQueue(string queueName);
		IWriteMessageQueue GetWriteQueue(string queueName);
	}
}