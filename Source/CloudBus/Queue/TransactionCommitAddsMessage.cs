using System.Transactions;
using Microsoft.WindowsAzure.StorageClient;

namespace Bus2.Queue
{
	public class TransactionCommitAddsMessage : IEnlistmentNotification
	{
		readonly CloudQueue _queue;
		readonly CloudQueueMessage _message;

		public TransactionCommitAddsMessage(CloudQueue queue, CloudQueueMessage message)
		{
			_queue = queue;
			_message = message;
		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		public void Commit(Enlistment enlistment)
		{
			_queue.AddMessage(_message);
			enlistment.Done();
		}

		public void Rollback(Enlistment enlistment)
		{
			enlistment.Done();
		}

		public void InDoubt(Enlistment enlistment)
		{
			enlistment.Done();
		}
	}
}