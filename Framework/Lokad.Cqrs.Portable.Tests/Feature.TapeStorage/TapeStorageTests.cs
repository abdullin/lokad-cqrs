using System;
using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public abstract class TapeStorageTests
    {
        // ReSharper disable InconsistentNaming

        protected ITapeStream _stream { get; private set; }

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
            _stream = InitializeAndGetTapeStorage();
        }

        [TearDown]
        public void TestTearDown()
        {
            FreeResources();
            TearDownEnvironment();
        }

        protected abstract void PrepareEnvironment();
        protected abstract ITapeStream InitializeAndGetTapeStorage();
        protected abstract void FreeResources();
        protected abstract void TearDownEnvironment();

        [Test]
        public void Can_read_imediately_after_write()
        {
            foreach (var item in _batch.Select((r, i) => new { Version = i + 1, Data = r}))
            {
                _stream.TryAppend(item.Data);

                var reading = _stream.ReadRecords(item.Version, 1).ToArray();

                Assert.AreEqual(1, reading.Length, "Number of records mismatch");
                Assert.AreEqual(item.Version, reading[0].Version, "Version mismatch");
                CollectionAssert.AreEqual(item.Data, reading[0].Data, "Data mismatch");
            }
        }

        [Test]
        public void Returns_correct_version()
        {
            Assert.AreEqual(0, _stream.GetCurrentVersion());
            foreach (var item in _batch.Select((r, i) => new { Version = i + 1, Data = r }))
            {
                _stream.TryAppend(item.Data);
                Assert.AreEqual(item.Version, _stream.GetCurrentVersion());
            }
        }

        [Test]
        public void Reading_empty_storage_returns_none()
        {
            TearDownEnvironment();
            CollectionAssert.IsEmpty(_stream.ReadRecords(1, 10));
            Assert.AreEqual(0, _stream.GetCurrentVersion());
        }

        [Test]
        public void Reading_batch_by_one()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var readings = Enumerable
                .Range(1, _batch.Length)
                .Select(v => new { Version = v, Records = _stream.ReadRecords(v, 1).ToArray()});

            foreach (var reading in readings)
            {
                Assert.AreEqual(1, reading.Records.Length, "Number of records mismatch");
                Assert.AreEqual(reading.Version, reading.Records[0].Version, "Version mismatch");
                CollectionAssert.AreEqual(_batch[reading.Version - 1], reading.Records[0].Data, "Data mismatch");
            }
        }

        [Test]
        public void Reading_batch_at_once()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var readings = _stream.ReadRecords(1, _batch.Length).ToArray();

            Assert.AreEqual(_batch.Length, readings.Length, "Number of records mismatch");

            var version = 1;
            foreach (var reading in readings)
            {
                Assert.AreEqual(version++, reading.Version, "Version mismatch");
                CollectionAssert.AreEqual(_batch[reading.Version - 1], reading.Data, "Data mismatch");
            }
        }

        [Test]
        public void Can_continue_after_recreation()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            _stream = null;
            FreeResources();

            _stream = InitializeAndGetTapeStorage();

            var version = _batch.Length + 1;
            _stream.TryAppend(_batch[0]);

            var readings = _stream.ReadRecords(version, 1).ToArray();

            Assert.AreEqual(1, readings.Length, "Number of records mismatch");
            var reading = readings[0];
            Assert.AreEqual(version, reading.Version, "Version mismatch");
            CollectionAssert.AreEqual(_batch[0], reading.Data, "Data mismatch");
        }

        [Test]
        public void Reading_ahead_storage_returns_none()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var version = _batch.Length + 1;
            var readings = _stream.ReadRecords(version, 1).ToArray();

            Assert.AreEqual(0, readings.Length, "Number of records mismatch");
        }

        [Test]
        public void Reading_ahead_returns_only_written()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var version = _batch.Length - 1;
            var readings = _stream.ReadRecords(version, _batch.Length).ToArray();

            Assert.AreEqual(2, readings.Length, "Number of records mismatch");

            foreach (var reading in readings)
            {
                Assert.AreEqual(version++, reading.Version, "Version mismatch");
                CollectionAssert.AreEqual(_batch[reading.Version - 1], reading.Data, "Data mismatch");
            }
        }

        [Test]
        public void Specified_condition_is_verified_for_empty_storage()
        {
            var previousVersion = _stream.GetCurrentVersion();
            Assert.AreEqual(0, previousVersion, "Version should be zero");

            var result = _stream.TryAppend(_batch[0], TapeAppendCondition.VersionIs(1));
            Assert.IsFalse(result, "Appending records should fail");

            var currentVersion = _stream.GetCurrentVersion();
            Assert.AreEqual(previousVersion, currentVersion, "Version should not change");
        }

        [Test]
        public void Specified_condition_is_verified_for_non_empty_storage()
        {
            _stream.TryAppend(_batch[0]);
            var previousVersion = _stream.GetCurrentVersion();
            Assert.AreEqual(1, previousVersion, "Version should be equal to one");

            var result = _stream.TryAppend(_batch[1], TapeAppendCondition.VersionIs(0));
            Assert.IsFalse(result, "Appending records should fail");

            var currentVersion = _stream.GetCurrentVersion();
            Assert.AreEqual(previousVersion, currentVersion, "Version should not change");
        }

        [Test]
        public void Specified_condition_is_matched_for_empty_storage()
        {
            var version = _stream.GetCurrentVersion();

            var success = _stream.TryAppend(_batch[0], TapeAppendCondition.VersionIs(version));
            Assert.IsTrue(success, "Appending records should succeed");

            var currentVersion = _stream.GetCurrentVersion();
            Assert.AreNotEqual(version, currentVersion, "Version should change");
        }

        [Test]
        public void Specified_condition_is_matched_for_non_empty_storage()
        {
            _stream.TryAppend(_batch[0]);
            var before = _stream.GetCurrentVersion();

            var success = _stream.TryAppend(_batch[1], TapeAppendCondition.VersionIs(before));
            Assert.IsTrue(success, "Appending records should succeed");

            var currentVersion = _stream.GetCurrentVersion();
            Assert.AreNotEqual(before, currentVersion, "Version should change");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Trying_to_append_null_buffer_causes_ANE()
        {
            _stream.TryAppend(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Trying_to_append_empty_buffer_causes_AE()
        {
            _stream.TryAppend(new byte[0]);
        }

        [Test]
        public void Reading_empty_storage_from_the_beginning_works()
        {
            var tapeRecords = _stream.ReadRecords(0, 1).ToArray();
            CollectionAssert.IsEmpty(tapeRecords);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Trying_to_read_negative_version_is_not_allowed()
        {
            _stream.ReadRecords(-1, 1).ToList();
        }
    }
}
