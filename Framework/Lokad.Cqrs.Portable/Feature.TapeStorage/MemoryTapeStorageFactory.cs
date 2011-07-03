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


        public ITapeReader GetReader(string name)
        {
            return new MemoryTapeReader(() =>
                {
                    List<byte[]> list;
                    if (_storage.TryGetValue(name, out list))
                    {
                        return list.ToArray();
                    }
                    return new byte[0][];
                });
        }
    }
}