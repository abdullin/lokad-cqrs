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
        MemoryTapeStorageFactory _storageFactory;

        protected override void PrepareEnvironment()
        {
        }

        protected override Factories GetTapeStorageInterfaces()
        {
            _storageFactory = new MemoryTapeStorageFactory(_storage);
            

            const string name = "Memory";

            return new Factories
            {
                Writer = _storageFactory.GetOrCreateWriter(name),
                Reader = _storageFactory.GetReader(name)
            };
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            _storage.Clear();
        }
    }
}