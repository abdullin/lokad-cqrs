using System;
using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class FileTapeStorageFactory : ITapeStorageFactory 
    {
        readonly string _fullPath;
        readonly ConcurrentDictionary<string, ITapeStream> _writers =
    new ConcurrentDictionary<string, ITapeStream>();


        public FileTapeStorageFactory(string fullPath)
        {
            _fullPath = fullPath;
        }

        public ITapeStream GetOrCreateStream(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var writer = _writers.GetOrAdd(
                name,
                n => new FileTapeStream(Path.Combine(_fullPath, name)));

            return writer;
        }

        public void InitializeForWriting()
        {
            Directory.CreateDirectory(_fullPath);
        }
    }
}