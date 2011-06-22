using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public sealed class MemoryTapeStorageTests : TapeStorageTests
    {
        // ReSharper disable InconsistentNaming

        readonly ConcurrentDictionary<string, List<byte[]>> _storage = new ConcurrentDictionary<string, List<byte[]>>();
        MemoryTapeReaderFactory _readerFactory;
        SingleThreadMemoryTapeWriterFactory _writerFactory;

        protected override void PrepareEnvironment()
        {
        }

        protected override Factories GetTapeStorageInterfaces()
        {
            _readerFactory = new MemoryTapeReaderFactory(_storage);
            _writerFactory = new SingleThreadMemoryTapeWriterFactory(_storage);

            const string name = "Memory";

            return new Factories
            {
                Writer = _writerFactory.GetOrCreateWriter(name),
                Reader = _readerFactory.GetReader(name)
            };
        }

        protected override void FreeResources()
        {
            _readerFactory = null;
            _writerFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            _storage.Clear();
        }
    }
}