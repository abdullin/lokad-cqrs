using System;
using System.Transactions;

namespace Lokad.Cqrs.Synthetic
{
    public sealed class TransactionTester : IEnlistmentNotification
    {
        public Action OnCommit = () => { };
        public Action OnRollback = () => { };


        public TransactionTester()
        {
            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            OnCommit();
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            OnRollback();
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}