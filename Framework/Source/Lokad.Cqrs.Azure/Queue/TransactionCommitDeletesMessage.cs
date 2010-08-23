#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Transactions;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Queue
{
	public class TransactionCommitDeletesMessage : IEnlistmentNotification
	{
		readonly CloudQueueMessage _message;
		readonly CloudQueue _queue;


		public TransactionCommitDeletesMessage(CloudQueue queue, CloudQueueMessage message)
		{
			Enforce.Arguments(() => queue, () => message);
			_queue = queue;
			_message = message;

		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		public void Commit(Enlistment enlistment)
		{
			try
			{
				_queue.DeleteMessage(_message);
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