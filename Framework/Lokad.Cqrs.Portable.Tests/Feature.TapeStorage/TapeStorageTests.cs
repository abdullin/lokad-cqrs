using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public abstract class TapeStorageTests
    {
        // ReSharper disable InconsistentNaming

        ITapeStream _stream;

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

            _stream = GetTapeStorageInterfaces();
        }

        [TearDown]
        public void TestTearDown()
        {
            FreeResources();
            CleanupEnvironment();
        }

        protected abstract void PrepareEnvironment();
        protected abstract ITapeStream GetTapeStorageInterfaces();
        protected abstract void FreeResources();
        protected abstract void CleanupEnvironment();

        [Test]
        public void Can_read_imediately_after_write()
        {
            foreach (var item in _batch.Select((r, i) => new { Index = i, Record = r}))
            {
                _stream.AppendRecords(new[] {item.Record});

                var reading = _stream.ReadRecords(item.Index, 1).ToArray();

                Assert.AreEqual(1, reading.Length, "Number of records mismatch");
                Assert.AreEqual(item.Index, reading[0].Index, "Index mismatch");
                CollectionAssert.AreEqual(item.Record, reading[0].Data, "Data mismatch");

                Assert.AreEqual(item.Index + 1, _stream.GetCurrentCount());
            }
        }

        [Test]
        public void Reading_empty_storage_returns_none()
        {
            CleanupEnvironment();
            CollectionAssert.IsEmpty(_stream.ReadRecords(0, 10));
            Assert.AreEqual(0, _stream.GetCurrentCount());
        }

        [Test]
        public void Reading_batch_by_one()
        {
            _stream.AppendRecords(_batch);

            var readings = Enumerable
                .Range(0, _batch.Length)
                .Select(i => new { Index = i, Records = _stream.ReadRecords(i, 1).ToArray()});

            foreach (var reading in readings)
            {
                Assert.AreEqual(1, reading.Records.Length, "Number of records mismatch");
                Assert.AreEqual(reading.Index, reading.Records[0].Index, "Index mismatch");
                CollectionAssert.AreEqual(_batch[reading.Index], reading.Records[0].Data, "Data mismatch");
            }
        }

        [Test]
        public void Reading_batch_at_once()
        {
            _stream.AppendRecords(_batch);

            var readings = _stream.ReadRecords(0, _batch.Length).ToArray();

            Assert.AreEqual(_batch.Length, readings.Length, "Number of records mismatch");

            var index = 0;
            foreach (var reading in readings)
            {
                Assert.AreEqual(index++, reading.Index, "Index mismatch");
                CollectionAssert.AreEqual(_batch[reading.Index], reading.Data, "Data mismatch");
            }
        }

        [Test]
        public void Can_continue_after_recreation()
        {
            _stream.AppendRecords(_batch);

            _stream = null;
            FreeResources();

            _stream = GetTapeStorageInterfaces();

            _stream.AppendRecords(new[] {_batch[0]});

            var readings = _stream.ReadRecords(_batch.Length, 1).ToArray();

            Assert.AreEqual(1, readings.Length, "Number of records mismatch");
            var reading = readings[0];
            Assert.AreEqual(_batch.Length, reading.Index, "Index mismatch");
            CollectionAssert.AreEqual(_batch[0], reading.Data, "Data mismatch");
        }

        [Test]
        public void Reading_ahead_storage_returns_none()
        {
            _stream.AppendRecords(_batch);

            var readings = _stream.ReadRecords(_batch.Length, 1).ToArray();

            Assert.AreEqual(0, readings.Length, "Number of records mismatch");
        }

        [Test]
        public void Reading_ahead_returns_only_written()
        {
            _stream.AppendRecords(_batch);

            var readings = _stream.ReadRecords(_batch.Length - 2, _batch.Length).ToArray();

            Assert.AreEqual(2, readings.Length, "Number of records mismatch");

            var index = _batch.Length - 2;
            foreach (var reading in readings)
            {
                Assert.AreEqual(index++, reading.Index, "Index mismatch");
                CollectionAssert.AreEqual(_batch[reading.Index], reading.Data, "Data mismatch");
            }
        }

        
    }
}
