using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Named tape stream, that usually matches to an aggregate instance
    /// </summary>
    public interface ITapeStream
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records with <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">The number of records to skip.</param>
        /// <param name="maxCount">The max number of records to load.</param>
        /// <returns>collection of blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount);

        /// <summary>
        /// Returns current storage version
        /// </summary>
        /// <returns>current version of the storage</returns>
        long GetCurrentVersion();

        /// <summary>
        /// Tries the append data to the tape storage, ensuring that
        /// the version condition is met (if the condition is specified).
        /// </summary>
        /// <param name="buffer">The data to append.</param>
        /// <param name="appendCondition">The append condition.</param>
        /// <returns>whether the data was appended</returns>
        bool TryAppend(byte[] buffer, TapeAppendCondition appendCondition = default(TapeAppendCondition));
    }
}