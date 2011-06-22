using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public abstract class TapeStorageTests
    {
        // ReSharper disable InconsistentNaming

        ISingleThreadTapeWriter _writer;
        ITapeReader _reader;

        readonly byte[][] _batch = new[]
                {
                    new byte[] {1, 2, 3, 4, 5},
                    new byte[] {255, 254, 253, 252, 251},
                    new byte[] {10, 20, 30, 40, 50},
                    new byte[] {15, 25, 35, 45, 55},
                };

        [SetUp]
        public void TestSetUp()
        {
            PrepareEnvironment();

            var configuration = GetTapeStorageInterfaces();
            _reader = configuration.Reader;
            _writer = configuration.Writer;
        }

        [TearDown]
        public void TestTearDown()
        {
            FreeResources();
            CleanupEnvironment();
        }

        protected abstract void PrepareEnvironment();
        protected abstract Factories GetTapeStorageInterfaces();
        protected abstract void FreeResources();
        protected abstract void CleanupEnvironment();

        [Test]
        public void Can_read_imediately_after_write()
        {
            foreach (var item in _batch.Select((r, i) => new { Index = i, Record = r}))
            {
                _writer.SaveRecords(new[] {item.Record});

                var reading = _reader.ReadRecords(item.Index, 1).ToArray();

                Assert.AreEqual(1, reading.Length, "Number of records mismatch");
                Assert.AreEqual(item.Index, reading[0].Index, "Index mismatch");
                CollectionAssert.AreEqual(item.Record, reading[0].Data, "Data mismatch");
            }
        }

        [Test]
        public void Reading_empty_storage_returns_none()
        {
            CollectionAssert.IsEmpty(_reader.ReadRecords(0,10));
        }

        [Test]
        public void Reading_batch_by_one()
        {
            _writer.SaveRecords(_batch);

            var readings = Enumerable
                .Range(0, _batch.Length)
                .Select(i => new { Index = i, Records = _reader.ReadRecords(i, 1).ToArray()});

            foreach (var reading in readings)
            {
                Assert.AreEqual(reading.Index, reading.Records[0].Index, "Index mismatch");
                CollectionAssert.AreEqual(_batch[reading.Index], reading.Records[0].Data, "Data mismatch");
            }
        }

        [Test]
        public void Reading_batch_at_once()
        {
            _writer.SaveRecords(_batch);

            var readings = _reader.ReadRecords(0, _batch.Length).ToArray();

            foreach (var reading in readings)
            {
                Assert.AreEqual(reading.Index, reading.Index, "Index mismatch");
                CollectionAssert.AreEqual(_batch[reading.Index], reading.Data, "Data mismatch");
            }
        }

        [Test]
        public void Can_continue_after_recreation()
        {
            _writer.SaveRecords(_batch);

            _reader = null;
            _writer = null;
            FreeResources();

            var interfaces = GetTapeStorageInterfaces();
            _writer = interfaces.Writer;
            _reader = interfaces.Reader;

            _writer.SaveRecords(new[] {_batch[0]});

            var readings = _reader.ReadRecords(_batch.Length, 1).ToArray();
            Assert.AreEqual(1, readings.Length);

            var reading = readings[0];

            Assert.AreEqual(_batch.Length, reading.Index, "Index mismatch");
            CollectionAssert.AreEqual(_batch[0], reading.Data, "Data mismatch");
        }

        protected struct Factories
        {
            public ISingleThreadTapeWriter Writer;
            public ITapeReader Reader;
        }
    }
}
