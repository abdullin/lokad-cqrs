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
                    new byte[] {15, 25, 35, 45, 55}
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
            var version = _stream.GetCurrentVersion();

            foreach (var item in _batch)
            {
                _stream.TryAppend(item);

                var reading = _stream.ReadRecords(version, 1).ToArray();

                Assert.AreEqual(1, reading.Length, "Number of records mismatch");
                Assert.Greater(reading[0].Version, version, "Version mismatch");
                CollectionAssert.AreEqual(item, reading[0].Data, "Data mismatch");

                version = reading[0].Version;
            }
        }

        [Test]
        public void Returns_correct_version()
        {
            var version = _stream.GetCurrentVersion();
            Assert.AreEqual(0, version);

            foreach (var item in _batch.Select((r, i) => new { Version = i + 1, Data = r }))
            {
                _stream.TryAppend(item.Data);
                var currentVersion = _stream.GetCurrentVersion();
                Assert.Greater(currentVersion, version, "Version mismatch");

                version = currentVersion;
            }
        }

        [Test]
        public void Reading_empty_storage_returns_none()
        {
            TearDownEnvironment();
            CollectionAssert.IsEmpty(_stream.ReadRecords(0, 10));
            Assert.AreEqual(0, _stream.GetCurrentVersion());
        }

        [Test]
        public void Reading_batch_by_one()
        {
            var version = _stream.GetCurrentVersion();

            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            foreach (var data in _batch) {
                var reading = _stream.ReadRecords(version, 1).ToArray();
                var currentVersion = reading[0].Version;

                Assert.AreEqual(1, reading.Length, "Number of records mismatch");
                Assert.Greater(currentVersion, version, "Version mismatch");
                CollectionAssert.AreEqual(data, reading[0].Data, "Data mismatch");

                version = currentVersion;
            }
        }

        [Test]
        public void Reading_batch_at_once()
        {
            var version = _stream.GetCurrentVersion();

            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var readings = _stream.ReadRecords(version, _batch.Length).ToArray();

            Assert.AreEqual(_batch.Length, readings.Length, "Number of records mismatch");

            var i = 0;
            foreach (var reading in readings)
            {
                var currentVersion = reading.Version;

                Assert.Greater(currentVersion, version, "Version mismatch");
                CollectionAssert.AreEqual(_batch[i], reading.Data, "Data mismatch");

                version = currentVersion;
                i++;
            }
        }

        [Test]
        public void Can_continue_after_recreation()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var version = _stream.GetCurrentVersion();

            _stream = null;
            FreeResources();

            _stream = InitializeAndGetTapeStorage();

            _stream.TryAppend(_batch[0]);

            var readings = _stream.ReadRecords(version, 1).ToArray();

            Assert.AreEqual(1, readings.Length, "Number of records mismatch");
            Assert.Greater(readings[0].Version, version, "Version mismatch");
            CollectionAssert.AreEqual(_batch[0], readings[0].Data, "Data mismatch");
        }

        [Test]
        public void Reading_ahead_storage_returns_none()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var version = _stream.GetCurrentVersion();

            var readings = _stream.ReadRecords(version, 1).ToArray();

            Assert.AreEqual(0, readings.Length, "Number of records mismatch");
        }

        [Test]
        public void Reading_ahead_returns_only_written()
        {
            long twoVersionsBack = 0;
            long oneVersonBack = 0;

            foreach (var b in _batch)
            {
                twoVersionsBack = oneVersonBack;
                oneVersonBack = _stream.GetCurrentVersion();

                _stream.TryAppend(b);
            }

            var readings = _stream.ReadRecords(twoVersionsBack, _batch.Length).ToArray();

            Assert.AreEqual(2, readings.Length, "Number of records mismatch");

            var version = twoVersionsBack;
            var i = _batch.Length - 2;
            foreach (var reading in readings)
            {
                var currentVersion = reading.Version;
                Assert.Greater(currentVersion, version, "Version mismatch");
                CollectionAssert.AreEqual(_batch[i], reading.Data, "Data mismatch");

                version = currentVersion;
                i++;
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
            var previousVersion = _stream.GetCurrentVersion();
            _stream.TryAppend(_batch[0]);
            
            var currentVersion1 = _stream.GetCurrentVersion();
            Assert.Greater(currentVersion1, previousVersion, "Version should be equal to one");

            var result = _stream.TryAppend(_batch[1], TapeAppendCondition.VersionIs(previousVersion));
            Assert.IsFalse(result, "Appending records should fail");

            var currentVersion2 = _stream.GetCurrentVersion();
            Assert.AreEqual(currentVersion1, currentVersion2, "Version should not change");
        }

        [Test]
        public void Specified_condition_is_matched_for_empty_storage()
        {
            var version = _stream.GetCurrentVersion();

            var success = _stream.TryAppend(_batch[0], TapeAppendCondition.VersionIs(version));
            Assert.IsTrue(success, "Appending records should succeed");

            var currentVersion = _stream.GetCurrentVersion();
            Assert.Greater(currentVersion, version, "Version should change");
        }

        [Test]
        public void Specified_condition_is_matched_for_non_empty_storage()
        {
            _stream.TryAppend(_batch[0]);
            var before = _stream.GetCurrentVersion();

            var success = _stream.TryAppend(_batch[1], TapeAppendCondition.VersionIs(before));
            Assert.IsTrue(success, "Appending records should succeed");

            var currentVersion = _stream.GetCurrentVersion();
            Assert.Greater(currentVersion, before, "Version should change");
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

        [Test]
        public void Reading_non_empty_storage_from_the_beginning_works()
        {
            foreach (var b in _batch)
            {
                _stream.TryAppend(b);
            }

            var reading = _stream.ReadRecords(0, 1).ToArray();

            Assert.AreEqual(1, reading.Length, "Number of records mismatch");
            Assert.Greater(reading[0].Version, 0, "Version mismatch");
            CollectionAssert.AreEqual(_batch[0], reading[0].Data, "Data mismatch");
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Trying_to_read_negative_version_is_not_allowed()
        {
            _stream.ReadRecords(-1, 1).ToList();
        }
    }
}
