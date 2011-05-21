#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Engine;
using Microsoft.WindowsAzure;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Azure_Configuration : Atomic_storage_scenarios
    {
        protected override void EngineConfig(CqrsEngineBuilder b)
        {
            var account = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), account);

            b.Azure(m =>
                {
                    m.AddAzureAccount(account);
                    m.AddAzureProcess("azure-dev", new[] {"test-incoming"}, c => c.QueueVisibility(1));
                    m.AddAzureSender("azure-dev", "test-incoming", x => x.IdGeneratorForTests());
                });
            b.Storage(m => m.AtomicIsInAzure(account, DefaultWithCustomConfig));
        }
    }
}