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
            TapeStreamUtil.CheckArgsForTryAppend(buffer);

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

        public void AppendNonAtomic(IEnumerable<TapeRecord> records)
        {
            TapeStreamUtil.CheckArgsForAppentNonAtomic(records);

            if (!records.Any())
                return;

            var number = 0;
            foreach (var record in records)
            {
                if (record.Data.Length == 0)
                    throw new ArgumentException("Record must contain at least one byte.");

                var result = TryAppend(record.Data, TapeAppendCondition.VersionIs(record.Version - 1));

                if (!result)
                    throw new InvalidOperationException(string.Format("Version mismatch. {0} records were saved successfully.", number));

                number++;
            }
        }

        public IEnumerable<TapeRecord> ReadRecords(long version, int maxCount)
        {
            TapeStreamUtil.CheckArgsForReadRecords(version, maxCount);

            List<byte[]> list;
            if (!_storage.TryGetValue(_name, out list))
                return Enumerable.Empty<TapeRecord>();

            var tapeRecords = list
                .Skip((int)version - 1)
                .Select((b, i) => new TapeRecord(i + version, b))
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