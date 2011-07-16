#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;

namespace Lokad.Cqrs.Feature.Dispatch.Directory
{
    /// <summary>
    /// Default implementation of the Lazy thread-safe context manager. It wires optional
    /// handler context (derived from the message transport information) back to the handler.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public sealed class MethodContextManager<TContext> : IMethodContextManager
        where TContext : class
    {
        readonly ThreadLocal<TContext> _context = new ThreadLocal<TContext>();

        readonly Func<ImmutableEnvelope, ImmutableMessage, TContext> _factory;

        public MethodContextManager(Func<ImmutableEnvelope, ImmutableMessage, TContext> factory)
        {
            _factory = factory;
        }

        public void SetContext(ImmutableEnvelope envelope, ImmutableMessage message)
        {
            _context.Value = _factory(envelope, message);
        }

        public void ClearContext()
        {
            _context.Value = null;
        }

        public object GetContextProvider()
        {
            return new Func<TContext>(() =>
                {
                    if (_context.Value == null)
                    {
                        throw new InvalidOperationException("Context is not set outside of message invocation");
                    }
                    return _context.Value;
                });
        }

    }
}