#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Reflection;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class MethodInvoker 
    {
        readonly Func<Type,Type, MethodInfo> _hint;
        readonly IMethodContextManager _context;

        [DebuggerNonUserCode]
        public void InvokeConsume(object messageHandler, ImmutableMessage item, ImmutableEnvelope envelope)
        {
            var handlerType = messageHandler.GetType();
            var content = item.Content;
            var messageType = item.MappedType;
            var consume = _hint(handlerType, messageType);
            try
            {
                _context.SetContext(envelope, item);
                consume.Invoke(messageHandler, new[] {content});
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

        public MethodInvoker(Func<Type, Type, MethodInfo> hint, IMethodContextManager context)
        {
            _hint = hint;
            _context = context;
        }
    }
}