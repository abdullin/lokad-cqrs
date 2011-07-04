namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Factory for storing blocks of data into append-only storage,
    /// that is easily to scale and replicate. This is the foundation
    /// for event sourcing.
    /// </summary>
    public interface ITapeStorageFactory
    {
        /// <summary>
        /// Gets or creates a new named stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <returns>new stream instance</returns>
        ITapeStream GetOrCreateStream(string name);
        /// <summary>
        /// Initializes this storage for writing
        /// </summary>
        void InitializeForWriting();
    }
}