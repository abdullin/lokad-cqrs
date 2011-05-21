using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public abstract class Atomic_storage_scenarios
    {
        [Test]
        public void When_atomic_config_is_requested()
        {
            new Engine_scenario_for_AtomicStorage_in_partition()
                .TestConfiguration(EngineConfig);
        }


        [Test]
        public void When_nuclear_config_is_requested()
        {
            new Engine_scenario_for_NuclearStorage_in_partition()
                .TestConfiguration(EngineConfig);
        }

        [Test]
        public void When_custom_view_domain()
        {
            new Engine_scenario_for_custom_view_domain()
                .TestConfiguration(EngineConfig);
        }

        protected abstract void EngineConfig(CqrsEngineBuilder b);

        protected static void DefaultWithCustomConfig(DefaultAtomicStorageStrategyBuilder builder)
        {
            builder.WhereEntity(type =>
                {
                    if (typeof(Define.AtomicEntity).IsAssignableFrom(type))
                        return true;
                    if (type.Name.Contains("CustomDomainView"))
                        return true;
                    return false;
                });
            builder.FolderForEntity(type1 => "test-" +type1.Name.ToLowerInvariant());
            builder.FolderForSingleton("test-singleton");
        }
    }
}