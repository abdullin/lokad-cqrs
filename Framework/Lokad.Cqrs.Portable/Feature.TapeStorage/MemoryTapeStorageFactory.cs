using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class MemoryTapeStorageFactory : ITapeStorageFactory
    {
        readonly ConcurrentDictionary<string, List<byte[]>> _storage;


        public MemoryTapeStorageFactory(ConcurrentDictionary<string, List<byte[]>> storage)
        {
            _storage = storage;
        }

        public void InitializeForWriting()
        {
        }
        public ITapeStream GetOrCreateStream(string name)
        {
            return new MemoryTapeStream(_storage, name);
        }
    }
}