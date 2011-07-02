using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    public abstract class All_synthetic_scenarios
    {
        protected abstract void CurrentConfig(CqrsEngineBuilder config);
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

        [Test]
        public void Command_batches_work_with_transaction()
        {
            new Engine_scenario_for_transactional_commands()
                .TestConfiguration(CurrentConfig);
        }
    }
}