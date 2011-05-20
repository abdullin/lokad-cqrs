using System;
using System.Diagnostics;
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
            if (Debugger.IsAttached)
            {
                Thread.CurrentThread.Name = string.Format("Consume: {0}", message.MappedType.Name);
            }
        }

        public void ClearContext()
        {
            _context.Value = null;
            if (Debugger.IsAttached)
            {
                Thread.CurrentThread.Name = "Consume: <Wait>";
            }
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