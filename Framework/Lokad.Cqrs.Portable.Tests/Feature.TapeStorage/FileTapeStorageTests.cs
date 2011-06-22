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

        protected override void PrepareEnvironment()
        {
            _path = Path.GetTempFileName();
            File.Delete(_path);
        }

        protected override Factories GetTapeStorageInterfaces()
        {
            _writerFactory = new SingleThreadFileTapeWriterFactory(_path);
            _writerFactory.Initialize();

            _readerFactory = new FileTapeReaderFactory(_path);

            const string name = "test";

            return new Factories
                {
                    Writer = _writerFactory.GetOrCreateWriter(name),
                    Reader = _readerFactory.GetReader(name)
                };
        }

        protected override void FreeResources()
        {
            _writerFactory = null;
            _readerFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            Directory.Delete(_path, true);
        }
    }
}
