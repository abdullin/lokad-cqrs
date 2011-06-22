using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SingleThreadMemoryTapeWriter : ISingleThreadTapeWriter
    {
        readonly Action<IEnumerable<byte[]>> _writer;

        public SingleThreadMemoryTapeWriter(Action<IEnumerable<byte[]>> writer)
        {
            _writer = writer;
        }

        public void SaveRecords(IEnumerable<byte[]> records)
        {
            _writer(records);
        }
    }
}