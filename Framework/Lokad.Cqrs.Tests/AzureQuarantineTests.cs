#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Scenarios;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    public static class AzureScenarios
    {
        // ReSharper disable InconsistentNaming

        [TestFixture]
        public sealed class AzureQuarantine : When_sending_failing_messages
        {
            public AzureQuarantine()
            {
                EnlistFixtureConfig(b => b.Azure(m =>
                    {
                        m.AddAzureAccount("azure-dev", CloudStorageAccount.DevelopmentStorageAccount);
                        m.AddAzureProcess("azure-dev", new[] {"incoming"}, c =>
                            {
                                c.QueueVisibilityTimeout(1);
                                c.WhenFactoryCreated(f => f.SetupForTesting());
                            });
                        m.AddAzureSender("azure-dev", "incoming", x => x.IdGeneratorForTests());
                    }));
            }
        }
    }
}