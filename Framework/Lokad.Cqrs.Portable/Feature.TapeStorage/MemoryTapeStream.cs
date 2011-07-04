using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class MemoryTapeStream : ITapeStream
    {
        readonly ConcurrentDictionary<string, List<byte[]>> _storage;
        readonly string _name;

        public MemoryTapeStream(ConcurrentDictionary<string, List<byte[]>> storage, string name)
        {
            _storage = storage;
            _name = name;


            /*
             * 
             * 
             * () =>
                {
                    List<byte[]> list;
                    if (_storage.TryGetValue(name, out list))
                    {
                        return list.ToArray();
                    }
                    return new byte[0][];
                }, blocks =>
                _storage.AddOrUpdate(name, s => new List<byte[]>(blocks),
                    (s1, list) =>
                    {
                        list.AddRange(blocks);
                        return list;
                    })
             * */
        }

        public bool TryAppendRecords(ICollection<byte[]> records, TapeAppendCondition condition)
        {
            if (records.Count == 0)
                return false;
            try
            {
                _storage.AddOrUpdate(_name, s =>
                    {
                        condition.Enforce(0);
                        return records.ToList();
                    }, (s, list) =>
                        {
                            condition.Enforce(list.Count);
                            list.AddRange(records);
                            return list;
                        });
                return true;
            }
            catch (TapeAppendException)
            {
                return false;
            }
        }

        public IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount)
        {
            List<byte[]> list;
            if (!_storage.TryGetValue(_name, out list))
            {
                return Enumerable.Empty<TapeRecord>();
            }
            var tapeRecords = list
                .Select((b,i) => new TapeRecord(i, b))
                .Skip((int)offset)
                .Take(maxCount)
                .ToArray();
            return tapeRecords;
        }

        public long GetCurrentVersion()
        {
            List<byte[]> list;
            if(_storage.TryGetValue(_name, out list))
            {
                return list.Count;
            }
            return 0;
        }
    }
}