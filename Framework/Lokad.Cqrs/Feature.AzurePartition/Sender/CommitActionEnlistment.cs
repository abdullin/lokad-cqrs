using System;
using System.Transactions;

namespace Lokad.Cqrs.Feature.AzureSender
{
	sealed class CommitActionEnlistment : IEnlistmentNotification
	{
		readonly Action _commit;

		public CommitActionEnlistment(Action commit)
		{
			_commit = commit;
		}

		public void Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		public void Commit(Enlistment enlistment)
		{
			_commit();
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