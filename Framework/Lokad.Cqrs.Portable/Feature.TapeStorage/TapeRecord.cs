namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Contains information about the committed data
    /// </summary>
    public sealed class TapeRecord
    {
        public readonly long Index;
        public readonly byte[] Data;

        public TapeRecord(long index, byte[] data)
        {
            Index = index;
            Data = data;
        }
    }
}