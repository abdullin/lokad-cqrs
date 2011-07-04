using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class MemoryTapeStream : ITapeStream
    {
        readonly Func<byte[][]> _getSnapshot;
        readonly Action<IEnumerable<byte[]>> _writer;

        public MemoryTapeStream(Func<byte[][]> getSnapshot, Action<IEnumerable<byte[]>> writer)
        {
            _getSnapshot = getSnapshot;
            _writer = writer;
        }

        public void AppendRecords(ICollection<byte[]> records)
        {
            _writer(records);
        }

        public IEnumerable<TapeRecord> ReadRecords(long version, int maxCount)
        {
            var snapshot = _getSnapshot();
            var tapeRecords = snapshot
                .Select((b,i) => new TapeRecord(i, b))
                .Skip((int)version)
                .Take(maxCount)
                .ToArray();
            return tapeRecords;
        }

        public long GetCurrentVersion()
        {
            return _getSnapshot().Length;
        }
    }
}