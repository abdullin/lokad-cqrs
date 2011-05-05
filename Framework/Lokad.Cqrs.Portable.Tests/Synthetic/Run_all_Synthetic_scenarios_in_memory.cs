using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Run_all_Synthetic_scenarios_in_memory
    {
        // ReSharper disable InconsistentNaming

        static void CurrentConfig(CqrsEngineBuilder config)
        {
            config.Memory(m =>
            {
                m.AddMemoryProcess("do");
                m.AddMemorySender("do", cb => cb.IdGeneratorForTests());
            });
        }

        [Test]
        public void Transient_failures_are_retried()
        {
            new Engine_scenario_for_transient_failure()
            .TestConfiguration(CurrentConfig);
        }

        [Test]
        public void Permanent_failure_is_quarantined()
        {
            new Engine_scenario_for_permanent_failure()
                .TestConfiguration(CurrentConfig);
        }
    }
}