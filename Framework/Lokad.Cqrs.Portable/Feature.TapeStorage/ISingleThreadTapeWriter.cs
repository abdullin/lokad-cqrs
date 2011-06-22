using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Single-threaded writer to the sequential storage
    /// </summary>
    public interface ISingleThreadTapeWriter
    {
        /// <summary>
        /// Saves the specified records
        /// </summary>
        /// <param name="records">The records to save.</param>
        void SaveRecords(IEnumerable<byte[]> records);
    }
}