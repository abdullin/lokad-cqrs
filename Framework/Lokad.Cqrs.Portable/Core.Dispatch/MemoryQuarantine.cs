using System;
using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Core.Dispatch
{
    public sealed class MemoryQuarantine : IEnvelopeQuarantine
    {
        readonly ConcurrentDictionary<string,int> _failures = new ConcurrentDictionary<string, int>();

        public bool TryToQuarantine(EnvelopeTransportContext context, Exception ex)
        {
            var current = _failures.AddOrUpdate(context.Unpacked.EnvelopeId, s => 1, (s1, i) => i + 1);
            if (current < 4)
            {
                return false;
            }
            // accept and forget
            int forget;
            _failures.TryRemove(context.Unpacked.EnvelopeId, out forget);
            return true;
        }

        public void TryRelease(EnvelopeTransportContext context)
        {
            int value;
            _failures.TryRemove(context.Unpacked.EnvelopeId, out value);
        }
    }
}