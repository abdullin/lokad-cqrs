namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeStorageFactory
    {
        ITapeStream GetOrCreateStream(string name);
        void InitializeForWriting();
    }
}