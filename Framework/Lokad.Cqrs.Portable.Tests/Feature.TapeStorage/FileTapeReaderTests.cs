using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class FileTapeReaderTests
    {
        [Test]
        public void CanReadImmediatelyAfterWrite()
        {
            const string name = "test";
            var path = Path.GetTempFileName();
            File.Delete(path);

            var writerFactory = new FileTapeWriterFactory(path);
            writerFactory.Init();

            try
            {
                var readerFactory = new FileTapeReaderFactory(path);

                var writer = writerFactory.GetOrCreateWriter(name);
                var reader = readerFactory.GetReader(name);

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
            finally
            {
                Directory.Delete(path, true);
            }

        }
    }
}
