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

        protected override ITapeStream InitializeAndGetTapeStorage()
        {
            _storageFactory = new FileTapeStorageFactory(_path);
            _storageFactory.InitializeForWriting();

            const string name = "test";
            return _storageFactory.GetOrCreateStream(name);
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
        }

        protected override void TearDownEnvironment()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
    }
}
