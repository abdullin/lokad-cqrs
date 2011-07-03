namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeStorageFactory
    {
        ITapeReader GetReader(string name);

        void Initialize();
        ISingleThreadTapeWriter GetOrCreateWriter(string name);
    }
}