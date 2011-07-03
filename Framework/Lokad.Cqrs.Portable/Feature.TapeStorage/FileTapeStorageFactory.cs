using System;
using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class FileTapeStorageFactory : ITapeStorageFactory 
    {
        readonly string _fullPath;
        readonly ConcurrentDictionary<string, ISingleThreadTapeWriter> _writers =
    new ConcurrentDictionary<string, ISingleThreadTapeWriter>();


        public FileTapeStorageFactory(string fullPath)
        {
            _fullPath = fullPath;
        }

        public ITapeReader GetReader(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var reader = new FileTapeReader(Path.Combine(_fullPath, name));

            return reader;
        }

        public void Initialize()
        {
            Directory.CreateDirectory(_fullPath);
        }

        public ISingleThreadTapeWriter GetOrCreateWriter(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var writer = _writers.GetOrAdd(
                name,
                n => new SingleThreadFileTapeWriter(Path.Combine(_fullPath, name)));

            return writer;
        }

    }
}