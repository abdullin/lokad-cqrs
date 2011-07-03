using System.IO;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class FileTapeStorageTests : TapeStorageTests
    {
        string _path;
        
        ITapeStorageFactory _storageFactory;

        protected override void PrepareEnvironment()
        {
            _path = Path.GetTempFileName();
            File.Delete(_path);
        }

        protected override Factories GetTapeStorageInterfaces()
        {
            _storageFactory = new FileTapeStorageFactory(_path);
            _storageFactory.Initialize();

            const string name = "test";

            return new Factories
                {
                    Writer = _storageFactory.GetOrCreateWriter(name),
                    Reader = _storageFactory.GetReader(name)
                };
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
    }
}
