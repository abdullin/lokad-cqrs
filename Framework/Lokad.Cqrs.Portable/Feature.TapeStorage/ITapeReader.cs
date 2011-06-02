using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeReader
    {
        /// <summary>
        /// Reads up to <see cref="maxCount"/> records starting from the <see cref="index"/>.
        /// </summary>
        /// <param name="index">The index to start reading from.</param>
        /// <param name="maxCount">The max count of records to load.</param>
        /// <returns>collection of taped blocks</returns>
        IEnumerable<TapeRecord> ReadRecords(int index, int maxCount);
    }
}