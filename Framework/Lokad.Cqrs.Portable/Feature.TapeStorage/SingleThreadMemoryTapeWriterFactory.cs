using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SingleThreadMemoryTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        readonly ConcurrentDictionary<string, List<byte[]>> _storage;
        public SingleThreadMemoryTapeWriterFactory(ConcurrentDictionary<string, List<byte[]>> storage)
        {
            _storage = storage;
        }

        public void Initialize()
        {
        }

        public ISingleThreadTapeWriter GetOrCreateWriter(string name)
        {
            return new SingleThreadMemoryTapeWriter(blocks =>
                _storage.AddOrUpdate(name, s => new List<byte[]>(blocks),
                    (s1, list) =>
                        {
                            list.AddRange(blocks);
                            return list;
                        }));
        }
    }
}