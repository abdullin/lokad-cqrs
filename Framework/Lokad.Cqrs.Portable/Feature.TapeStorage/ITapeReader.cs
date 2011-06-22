using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeReader
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records with <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">The number of records to skip.</param>
        /// <param name="maxCount">The max number of records to load.</param>
        /// <returns>collection of taped blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount);
    }
}