using System;
using System.Diagnostics;
using System.Transactions;

namespace Lokad.Cqrs.Evil
{
    public static class TransactionEvil
    {
        public static Func<TransactionScope> NoTransactions()
        {
            return () => new TransactionScope(TransactionScopeOption.Suppress);
        }
        public static Func<TransactionScope> Transactional(TransactionScopeOption option, IsolationLevel level = IsolationLevel.Serializable, TimeSpan timeout = default(TimeSpan))
        {
            if (timeout == (default(TimeSpan)))
            {
                timeout = TimeSpan.FromMinutes(10);
            }
            return () => new TransactionScope(option, new TransactionOptions()
                {
                    IsolationLevel = level,
                    Timeout = Debugger.IsAttached ? TimeSpan.MaxValue : timeout
                });
        }
    }
}