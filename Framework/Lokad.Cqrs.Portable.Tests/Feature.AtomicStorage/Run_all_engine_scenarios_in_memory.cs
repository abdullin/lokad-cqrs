#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Run_all_engine_scenarios_in_memory
    {
        [Test]
        public void When_atomic_config_is_requested()
        {
            new Engine_scenario_for_AtomicStorage_in_partition()
                .TestConfiguration(CurrentConfig);
        }


        [Test]
        public void When_nuclear_config_is_requested()
        {
            new Engine_scenario_for_NuclearStorage_in_partition()
                .TestConfiguration(CurrentConfig);
        }

        [Test]
        public void When_custom_view_domain()
        {
            new Engine_scenario_for_custom_view_domain()
                .TestConfiguration(CurrentConfig);
        }

        static void CurrentConfig(CqrsEngineBuilder config)
        {
            config.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemorySender("do", cb => cb.IdGeneratorForTests());
                });
        }
    }
}