namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Contains information about the committed data
    /// </summary>
    public sealed class TapeRecord
    {
        public readonly long Version;
        public readonly byte[] Data;

        public TapeRecord(long version, byte[] data)
        {
            Version = version;
            Data = data;
        }
    }
}