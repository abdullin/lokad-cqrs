#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Core.Dispatch
{
    public sealed class AutofacDispatchStrategy : IMessageDispatchStrategy
    {
        readonly ILifetimeScope _scope;
        readonly Func<TransactionScope> _scopeFactory;
        readonly Func<Type, Type, MethodInfo> _hint;
        readonly IMethodContextManager _context;


        public AutofacDispatchStrategy(ILifetimeScope scope, Func<TransactionScope> scopeFactory, Func<Type, Type, MethodInfo> hint, IMethodContextManager context)
        {
            _scope = scope;
            _scopeFactory = scopeFactory;
            _hint = hint;
            _context = context;
        }

        public IMessageDispatchScope BeginEnvelopeScope()
        {
            var outer = _scope.BeginLifetimeScope(DispatchLifetimeScopeTags.MessageEnvelopeScopeTag);
            var tx = _scopeFactory();

            return new AutofacMessageDispatchScope(tx, outer, _hint,_context);
        }

        sealed class AutofacMessageDispatchScope : IMessageDispatchScope
        {
            readonly TransactionScope _tx;
            readonly ILifetimeScope _envelopeScope;
            readonly Func<Type, Type, MethodInfo> _hint;
            readonly IMethodContextManager _context;
            
            public AutofacMessageDispatchScope(TransactionScope tx, ILifetimeScope envelopeScope, Func<Type, Type, MethodInfo> hint, IMethodContextManager context)
            {
                _tx = tx;
                _envelopeScope = envelopeScope;
                _hint = hint;
                _context = context;
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
                    var instance = inner.Resolve(consumerType);
                    var consume = _hint(consumerType, message.MappedType);
                    try
                    {
                        _context.SetContext(envelop, message);
                        consume.Invoke(instance, new[] { message.Content });
                    }
                    catch (TargetInvocationException e)
                    {
                        throw InvocationUtil.Inner(e);
                    }
                    finally
                    {
                        _context.ClearContext();
                    }
                }
            }


            public void Complete()
            {
                _tx.Complete();
            }
        }
    }
}