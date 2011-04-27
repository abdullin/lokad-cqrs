using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Scenarios;
using Lokad.Cqrs.Tests;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    public static class AzureScenarios
    {
        // ReSharper disable InconsistentNaming

        public sealed class AzureConfig : IConfigureEngineForFixture
        {
            public void Config(CloudEngineBuilder builder)
            {
                builder.Azure(m =>
                    {
                        m.AddAzureAccount("azure-dev", CloudStorageAccount.DevelopmentStorageAccount);
                        m.AddAzureProcess("azure-dev", new []{"incoming"},c =>
                            {
                                c.QueueVisibilityTimeout(1);
                                c.WhenFactoryCreated(f => f.SetupForTesting());
                            });
                        m.AddAzureSender("azure-dev", "incoming");
                        
                    });
                
            }
        }

        [TestFixture]
        public sealed class AzureQuarantine : When_sending_failing_messages<AzureConfig> { }
    }
}