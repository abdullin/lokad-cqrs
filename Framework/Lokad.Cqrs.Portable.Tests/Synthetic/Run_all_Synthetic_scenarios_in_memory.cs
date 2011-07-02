using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Run_all_Synthetic_scenarios_in_memory : All_synthetic_scenarios
    {
        // ReSharper disable InconsistentNaming

        protected override void CurrentConfig(CqrsEngineBuilder config)
        {
            config.Memory(m =>
            {
                m.AddMemoryProcess("do", x => x.DispatchAsCommandBatch());
                m.AddMemorySender("do", cb => cb.IdGeneratorForTests());
            });
        }

   
    }
}