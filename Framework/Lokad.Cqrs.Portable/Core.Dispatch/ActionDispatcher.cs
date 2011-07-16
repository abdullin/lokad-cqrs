using System;

namespace Lokad.Cqrs.Core.Dispatch
{
    public sealed class ActionDispatcher : ISingleThreadMessageDispatcher
    {
        readonly Action<ImmutableEnvelope> _dispatcher;
        public ActionDispatcher(Action<ImmutableEnvelope> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void DispatchMessage(ImmutableEnvelope message)
        {
            _dispatcher(message);
        }

        public void Init()
        {
            
        }
    }
}