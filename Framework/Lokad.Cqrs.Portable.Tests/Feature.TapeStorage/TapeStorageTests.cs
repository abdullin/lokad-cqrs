using System;
using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public abstract class TapeStorageTests
    {
        ISingleThreadTapeWriter _writer;
        ITapeReader _reader;

        [SetUp]
        public void TestSetUp()
        {
            SetUp();

            var configuration = GetConfiguration();
            var name = configuration.Name;
            _reader = configuration.ReaderFactory.GetReader(name);
            _writer = configuration.WriterFactory.GetOrCreateWriter(name);
        }

        [TearDown]
        public void TestTearDown()
        {
            TearDown();
        }

        protected abstract void SetUp();
        protected abstract void TearDown();
        protected abstract TestConfiguration GetConfiguration();

        [Test]
        public void CanReadImmediatelyAfterWrite()
        {
            var rand = new Random();
            var recordsCount = 1;// rand.Next(1000) + 1;
            var records = Enumerable.Range(1, recordsCount)
                .Select(a => Enumerable.Range(1, rand.Next(1000) + 1)
                    .Select(b => (byte)rand.Next(byte.MaxValue + 1))
                    .ToArray())
                .ToArray();

            var ix = 1;
            foreach (var record in records)
            {
                _writer.SaveRecords(new[] { record });
                var read = _reader.ReadRecords(ix, 1).ToArray();
                Assert.AreEqual(1, read.Length);
                Assert.AreEqual(ix, read[0].Index);
                Assert.AreEqual(record, read[0].Data);

                ix++;
            }
        }

        protected struct TestConfiguration
        {
            public string Name;
            public ISingleThreadTapeWriterFactory WriterFactory;
            public ITapeReaderFactory ReaderFactory;
        }
    }
}
