using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class FileTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        readonly string _fullPath;
        readonly ConcurrentDictionary<string, ISingleThreadTapeWriter> _writers =
            new ConcurrentDictionary<string, ISingleThreadTapeWriter>();

        public FileTapeWriterFactory(string fullPath)
        {
            _fullPath = fullPath;
        }

        public void Init()
        {
            Directory.CreateDirectory(_fullPath);
        }

        public ISingleThreadTapeWriter GetOrCreateWriter(string name)
        {
            var writer = _writers.GetOrAdd(
                name,
                n => new SingleThreadFileTapeWriter(Path.Combine(_fullPath, name)));

            return writer;
        }
    }
}