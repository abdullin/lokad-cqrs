using System.IO;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class FileTapeStorageTests : TapeStorageTests
    {
        string _path;
        SingleThreadFileTapeWriterFactory _writerFactory;
        ITapeReaderFactory _readerFactory;

        protected override void SetUp()
        {
            _path = Path.GetTempFileName();
            File.Delete(_path);

            _writerFactory = new SingleThreadFileTapeWriterFactory(_path);
            _writerFactory.Init();

            _readerFactory = new FileTapeReaderFactory(_path);
        }

        protected override void TearDown()
        {
            Directory.Delete(_path, true);
        }

        protected override TestConfiguration GetConfiguration()
        {
            return new TestConfiguration
                {
                    Name = "test",
                    WriterFactory = _writerFactory,
                    ReaderFactory = _readerFactory
                };
        }
    }
}
