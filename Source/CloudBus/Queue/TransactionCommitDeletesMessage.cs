using System.Transactions;
using Lokad;
using Microsoft.WindowsAzure.StorageClient;

namespace Bus2.Queue
{
	public class TransactionCommitDeletesMessage : IEnlistmentNotification
	{
		readonly CloudQueue _queue;
		readonly string _messageId;
		readonly string _popId;


		public TransactionCommitDeletesMessage(CloudQueue queue, string messageId, string popId)
		{
			Enforce.Arguments(() => queue, () => messageId, () => popId);
			_queue = queue;
			_messageId = messageId;
			_popId = popId;
		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		public void Commit(Enlistment enlistment)
		{
			try
			{
				_queue.DeleteMessage(_messageId, _popId);
			}
			catch (StorageClientException ex)
			{
				if (ex.ExtendedErrorInformation.ErrorCode != "MessageNotFound")
					throw;
			}

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