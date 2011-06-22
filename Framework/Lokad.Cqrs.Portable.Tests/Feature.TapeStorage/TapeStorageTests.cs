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

            var testsCount = rand.Next(50) + 1;

            var recordSets = Enumerable.Range(1, testsCount)
                .Select(c => Enumerable.Range(1, rand.Next(5) + 1)
                    .Select(a => Enumerable.Range(1, rand.Next(1000) + 1)
                        .Select(b => (byte)rand.Next(byte.MaxValue + 1))
                        .ToArray())
                    .ToArray())
                .ToArray();

            var ix = 1;
            foreach (var records in recordSets)
            {
                _writer.SaveRecords(records);

                var offset = rand.Next(3);
                var count = records.Length + rand.Next(5) - 2;
                if (count < 1)
                    count = 1;

                while (offset + count > records.Length)
                {
                    if (rand.Next(2) == 0)
                    {
                        if (count > 1)
                            count--;
                    }
                    else
                    {
                        if (offset > 0)
                            offset--;
                    }
                }

                var readRecords = _reader.ReadRecords(ix + offset-1, count).ToArray();

                var mustReadRecords = Math.Min(count, records.Length);

                Assert.AreEqual(mustReadRecords, readRecords.Length, "Number of records mismatch");
                Assert.AreEqual(ix + offset-1, readRecords[0].Index, "Index mismatch");
                var expectedRecords = records.Skip(offset).Take(mustReadRecords).ToArray();
                var actualRecords = readRecords.Select(tr => tr.Data).ToArray();
                Assert.AreEqual(expectedRecords, actualRecords, "Data mismatch");

                ix += records.Length;
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
