using System;
using System.IO;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class FileTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        readonly string _fullPath;

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
            throw new NotImplementedException();
        }
    }
}