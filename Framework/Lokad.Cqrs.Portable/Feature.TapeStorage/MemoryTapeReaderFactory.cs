using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class MemoryTapeReaderFactory : ITapeReaderFactory
    {
        readonly ConcurrentDictionary<string, List<byte[]>> _storage;


        public MemoryTapeReaderFactory(ConcurrentDictionary<string, List<byte[]>> storage)
        {
            _storage = storage;
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