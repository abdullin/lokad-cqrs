namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// Contains information about the committed data
    /// </summary>
    public sealed class TapeRecord
    {
        public readonly int Index;
        public readonly byte[] Data;

        public TapeRecord(int index, byte[] data)
        {
            Index = index;
            Data = data;
        }
    }
}