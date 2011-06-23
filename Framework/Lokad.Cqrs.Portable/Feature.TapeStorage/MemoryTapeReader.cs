using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class MemoryTapeReader : ITapeReader
    {
        readonly Func<byte[][]> _getSnapshot;
        public MemoryTapeReader(Func<byte[][]> getSnapshot)
        {
            _getSnapshot = getSnapshot;
        }

        public IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount)
        {
            var snapshot = _getSnapshot();
            var tapeRecords = snapshot
                .Select((b,i) => new TapeRecord(i, b))
                .Skip((int)offset)
                .Take(maxCount)
                .ToArray();
            return tapeRecords;
        }

        public long Count
        {
            get { return _getSnapshot().Length; }
        }
    }
}