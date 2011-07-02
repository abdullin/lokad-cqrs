using System.IO;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Run_all_synthetic_scenarios_for_files : All_synthetic_scenarios
    {
        protected override void CurrentConfig(CqrsEngineBuilder config)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var bus = Path.Combine(currentDirectory, "test");
            var dir = new DirectoryInfo(bus);
            if (dir.Exists)
            {
                dir.Delete(true);
            }
            var store = FileStorage.CreateConfig(bus, "file");
            config.File(m =>
                {
                    m.AddFileProcess(store,"do", x => x.DispatchAsCommandBatch());
                    m.AddFileSender(store, "do", cb => cb.IdGeneratorForTests());
                });
        }
    }
}