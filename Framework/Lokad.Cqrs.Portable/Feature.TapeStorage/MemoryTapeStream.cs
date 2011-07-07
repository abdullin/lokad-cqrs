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
        }

        public bool TryAppend(byte[] buffer, TapeAppendCondition condition)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer must contain at least one byte.");

            try
            {
                _storage.AddOrUpdate(_name, s =>
                    {
                        condition.Enforce(0);
                        return new List<byte[]>
                            {
                                buffer
                            };
                    }, (s, list) =>
                        {
                            condition.Enforce(list.Count);
                            list.Add(buffer);
                            return list;
                        });
                return true;
            }
            catch (TapeAppendConditionException)
            {
                return false;
            }
        }

        public IEnumerable<TapeRecord> ReadRecords(long afterVersion, int maxCount)
        {
            if (afterVersion < 0)
                throw new ArgumentOutOfRangeException("afterVersion", "Must be zero or greater.");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "Must be more than zero.");

            // afterVersion + maxCount > long.MaxValue, but transformed to avoid overflow
            if (afterVersion > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Version will exceed long.MaxValue.");

            List<byte[]> list;
            if (!_storage.TryGetValue(_name, out list))
                return Enumerable.Empty<TapeRecord>();

            var tapeRecords = list
                .Skip((int)afterVersion)
                .Select((b, i) => new TapeRecord(i + afterVersion + 1, b))
                .Take(maxCount)
                .ToArray();

            return tapeRecords;
        }

        public long GetCurrentVersion()
        {
            List<byte[]> list;

            if(_storage.TryGetValue(_name, out list))
                return list.Count;

            return 0;
        }
    }
}