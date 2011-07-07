using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Named tape stream, that usually matches to an aggregate instance
    /// </summary>
    public interface ITapeStream
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records starting with version next to <see cref="afterVersion"/>.
        /// </summary>
        /// <param name="afterVersion">Number of version to start after.</param>
        /// <param name="maxCount">The max number of records to load.</param>
        /// <returns>collection of blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(long afterVersion, int maxCount);

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

        ///// <summary>
        ///// Appends block in non-transactional manner (used for high throughput copying).
        ///// It is the duty of the writer to verify that there are no concurrent changes
        ///// and that IO failures are handled. 
        ///// </summary>
        ///// <param name="records">The records to copy.</param>
        //void AppendNonAtomic(IEnumerable<TapeRecord> records);
    }
}