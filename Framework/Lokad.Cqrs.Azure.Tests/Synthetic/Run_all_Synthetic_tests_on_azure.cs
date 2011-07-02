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
    public sealed class Run_all_Synthetic_tests_on_azure : All_synthetic_scenarios
    {

        protected override void CurrentConfig(CqrsEngineBuilder b)
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
    }
}