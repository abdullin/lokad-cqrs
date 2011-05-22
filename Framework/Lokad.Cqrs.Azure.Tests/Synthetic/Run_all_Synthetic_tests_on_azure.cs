#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Synthetic;
using Microsoft.WindowsAzure;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Run_all_Synthetic_tests_on_azure
    {

        static void CurrentConfig(CqrsEngineBuilder b)
        {
            var dev = AzureStorage.CreateConfigurationForDev();
            
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), dev);
            b.Azure(m =>
                {
                    m.AddAzureProcess(dev, new[] {"test-incoming"}, c =>
                        {
                            c.QueueVisibility(1);
                            c.DispatchAsCommandBatch();
                        });
                    m.AddAzureSender(dev, "test-incoming", x => x.IdGeneratorForTests());
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

        [Test]
        public void Command_batches_work_with_transaction()
        {
            new Engine_scenario_for_transactional_commands()
            .TestConfiguration(CurrentConfig);
        }
        
    }
}