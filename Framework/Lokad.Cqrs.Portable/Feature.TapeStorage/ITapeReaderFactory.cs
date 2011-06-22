namespace Lokad.Cqrs.Feature.TapeStorage
{
    public interface ITapeReaderFactory
    {
        ITapeReader GetReader(string name);
    }
}