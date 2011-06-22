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

        protected override void SetUp()
        {
            _storage.Clear();
        }

        protected override void TearDown()
        {
            
        }

        

        protected override TestConfiguration GetConfiguration()
        {
            
            return new TestConfiguration()
                {
                    Name = "Memory",
                    ReaderFactory = new MemoryTapeReaderFactory(_storage),
                    WriterFactory = new SingleThreadMemoryTapeWriterFactory(_storage)

                };
        }


    }
}