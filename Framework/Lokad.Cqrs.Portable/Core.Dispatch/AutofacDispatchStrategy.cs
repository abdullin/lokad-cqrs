#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

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
        readonly MethodInvoker _invoker;

        public AutofacDispatchStrategy(ILifetimeScope scope, Func<TransactionScope> scopeFactory, MethodInvoker invoker)
        {
            _scope = scope;
            _scopeFactory = scopeFactory;
            _invoker = invoker;
        }

        public IMessageDispatchScope BeginEnvelopeScope()
        {
            var outer = _scope.BeginLifetimeScope(DispatchLifetimeScopeTags.MessageEnvelopeScopeTag);
            var tx = _scopeFactory();

            return new AutofacMessageDispatchScope(tx, outer, _invoker);
        }

        sealed class AutofacMessageDispatchScope : IMessageDispatchScope
        {
            readonly TransactionScope _tx;
            readonly ILifetimeScope _envelopeScope;
            readonly MethodInvoker _invoker;

            public AutofacMessageDispatchScope(TransactionScope tx, ILifetimeScope envelopeScope, MethodInvoker invoker)
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