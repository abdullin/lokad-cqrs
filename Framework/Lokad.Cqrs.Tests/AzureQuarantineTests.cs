using System;
using System.Linq;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Scenarios;
using NUnit.Framework;

namespace Lokad.Cqrs.Tests
{
    public static class Azure
    {
        // ReSharper disable InconsistentNaming

        public sealed class AzureConfig : IConfigureEngineForFixture
        {
            public void Config(CloudEngineBuilder builder)
            {
                builder.Azure(m =>
                    {
                        m.SendToAzureByDefault("in");
                        m.AddPartition("in");
                    });
            }
        }

        [TestFixture]
        public sealed class AzureQuarantine : When_sending_failing_messages<AzureConfig> { }
    }
}