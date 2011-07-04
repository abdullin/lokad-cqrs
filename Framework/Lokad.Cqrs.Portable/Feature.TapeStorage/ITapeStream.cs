using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeStream
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records with <see cref="version"/>.
        /// </summary>
        /// <param name="version">The number of records to skip.</param>
        /// <param name="maxCount">The max number of records to load.</param>
        /// <returns>collection of taped blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(long version, int maxCount);

        /// <summary>
        /// Returns current storage version
        /// </summary>
        /// <returns></returns>
        long GetVersion();

        /// <summary>
        /// Saves the specified records
        /// </summary>
        /// <param name="records">The records to save.</param>
        void AppendRecords(ICollection<byte[]> records);

    }
}