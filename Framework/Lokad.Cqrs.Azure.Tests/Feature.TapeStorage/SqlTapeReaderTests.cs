using System;
using System.Linq;
using Lokad.Cqrs.Properties;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class SqlTapeReaderTests
    {
        [Test]
        public void CanReadImmediatelyAfterWrite()
        {
            var connectionString = Settings.Default.SqlConnectionString;
            var tableName = Settings.Default.SqlTapeWriterTableName;
            const string streamName = "test";

            using (new TestDatabase(connectionString))
            {
                var writerFactory = new SqlTapeWriterFactory(connectionString, tableName);
                writerFactory.Init();

                var readerFactory = new SqlTapeReaderFactory(connectionString, tableName);

                var writer = writerFactory.GetOrCreateWriter(streamName);
                var reader = readerFactory.GetReader(streamName);

                var rand = new Random();
                var recordsCount = rand.Next(1000) + 1;
                var records = Enumerable.Range(1, recordsCount)
                    .Select(a => Enumerable.Range(1, rand.Next(1000) + 1)
                        .Select(b => (byte) rand.Next(byte.MaxValue + 1))
                        .ToArray())
                    .ToArray();

                var ix = 1;
                foreach (var record in records)
                {
                    writer.SaveRecords(new[] {record});
                    var read = reader.ReadRecords(ix, 1).ToArray();
                    Assert.AreEqual(1, read.Length);
                    Assert.AreEqual(ix, read[0].Index);
                    Assert.AreEqual(record, read[0].Data);

                    ix++;
                }
            }
        }
    }
}
