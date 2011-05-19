using System;
using System.Threading;

namespace Lokad.Cqrs.Core.Directory
{
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

        public TContext Get()
        {
            if (_context.Value == null)
            {
                throw new InvalidOperationException("Context is not set outside of message invocation");
            }
            return _context.Value;
        }
    }
}