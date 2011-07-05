using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public static class TapeStreamUtil
    {
        public static void CheckArgsForReadRecords(long version, int maxCount)
        {
            if (version <= 0)
                throw new ArgumentOutOfRangeException("version", "Must be more than zero.");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "Must be more than zero.");

            // version + maxCount > long.MaxValue, but transformed to avoid overflow
            if (version > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");
        }

        public static void CheckArgsForTryAppend(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer must contain at least one byte.");
        }

        public static void CheckArgsForAppentNonAtomic(IEnumerable<TapeRecord> records)
        {
            if (records == null)
                throw new ArgumentNullException("records");
        }
    }
}
