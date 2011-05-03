#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Build.Engine;
using Microsoft.WindowsAzure;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Run_all_AtomicStorage_scenarios_on_Azure
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

        static void CurrentConfig(CloudEngineBuilder b)
        {
            b.Azure(m =>
                {
                    m.AddAzureAccount("azure-dev", CloudStorageAccount.DevelopmentStorageAccount);
                    m.AddAzureProcess("azure-dev", new[] {"incoming"}, c => c.QueueVisibility(1));
                    m.AddAzureSender("azure-dev", "incoming", x => x.IdGeneratorForTests());
                    m.WipeAccountsAtStartUp = true;
                });
            b.Storage(m => m.AtomicStorageIsAzure("azure-dev", c => c.WithStrategy(DefaultWithCustomConfig)));
        }

        static void DefaultWithCustomConfig(DefaultAzureAtomicStorageStrategyBuilder builder)
        {
            builder.WhereEntity(type =>
                {
                    if (typeof (Define.AtomicEntity).IsAssignableFrom(type))
                        return true;
                    if (type.Name.Contains("CustomDomainView"))
                        return true;
                    return false;
                });
        }
    }
}