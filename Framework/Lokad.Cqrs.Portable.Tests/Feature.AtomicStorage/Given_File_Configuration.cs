using System.IO;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_File_Configuration : Atomic_storage_scenarios
    {
        // ReSharper disable InconsistentNaming

        readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), typeof (Given_File_Configuration).Name);
        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(_path))
            {
                Directory.Delete(_path, true);
            }
            Directory.CreateDirectory(_path);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            
        }

        protected override void EngineConfig(CqrsEngineBuilder b)
        {
            b.Storage(m => m.AtomicIsInFiles(_path, DefaultWithCustomConfig));
            b.Memory(m =>
                {
                    m.AddMemoryProcess("azure-dev");
                    m.AddMemorySender("azure-dev", x => x.IdGeneratorForTests());
                });
        }
        
    }
}