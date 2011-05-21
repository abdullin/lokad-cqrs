using System;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Core.Directory;

namespace Lokad.Cqrs.Core.Dispatch
{
    public sealed class AutofacDispatchStrategy : IMessageDispatchStrategy
    {
        readonly ILifetimeScope _scope;
        readonly Func<TransactionScope> _scopeFactory;
        readonly IMethodInvoker _invoker;
        
        public AutofacDispatchStrategy(ILifetimeScope scope, Func<TransactionScope> scopeFactory, IMethodInvoker invoker)
        {
            _scope = scope;
            _scopeFactory = scopeFactory;
            _invoker = invoker;
        }

        public IEnvelopeScope BeginEnvelopeScope()
        {
            var outer = _scope.BeginLifetimeScope(DispatchLifetimeScopeTags.MessageEnvelopeScopeTag);
            var tx = _scopeFactory();

            return new AutofacEnvelopeScope(tx, outer, _invoker);
        }

        sealed class AutofacEnvelopeScope : IEnvelopeScope
        {
            readonly TransactionScope _tx;
            readonly ILifetimeScope _envelopeScope;
            readonly IMethodInvoker _invoker;

            public AutofacEnvelopeScope(TransactionScope tx, ILifetimeScope envelopeScope, IMethodInvoker invoker)
            {
                _tx = tx;
                _envelopeScope = envelopeScope;
                _invoker = invoker;
            }

            public void Dispose()
            {
                _tx.Dispose();
                _envelopeScope.Dispose();
            }

            public void Dispatch(Type consumerType, ImmutableEnvelope envelop, ImmutableMessage message)
            {
                using (var inner = _envelopeScope.BeginLifetimeScope(DispatchLifetimeScopeTags.MessageItemScopeTag))
                {
                    var resolve = inner.Resolve(consumerType);
                    _invoker.InvokeConsume(resolve, message, envelop);
                }
            }

            public void Complete()
            {
                _tx.Complete();
            }
        }
    }
}