using System.IO;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class FileTapeReaderFactory : ITapeReaderFactory 
    {
        readonly string _fullPath;

        public FileTapeReaderFactory(string fullPath)
        {
            _fullPath = fullPath;
        }

        public ITapeReader GetReader(string name)
        {
            var reader = new SingleThreadFileTapeReader(Path.Combine(_fullPath, name));

            return reader;
        }
    }
}