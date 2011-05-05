using System;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Core.Directory;

namespace Lokad.Cqrs.Core.Dispatch
{
    public sealed class DispatchCommandBatchInTransaction : DispatchCommandBatch
    {
        readonly Func<TransactionScope> _factory;

        public DispatchCommandBatchInTransaction(ILifetimeScope container, MessageActivationMap messageDirectory, IMethodInvoker invoker, Func<TransactionScope> factory) : base(container, messageDirectory, invoker)
        {
            _factory = factory;
        }

        protected override void DispatchEnvelope(ImmutableEnvelope message)
        {
            using (var scope = _factory())
            {
                base.DispatchEnvelope(message);
                scope.Complete();
            }
        }
    }
}