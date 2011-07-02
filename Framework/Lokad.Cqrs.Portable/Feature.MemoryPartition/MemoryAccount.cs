using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryAccount
    {
        public readonly ConcurrentDictionary<string, BlockingCollection<ImmutableEnvelope>> Delivery =
            new ConcurrentDictionary<string, BlockingCollection<ImmutableEnvelope>>();
    }
}