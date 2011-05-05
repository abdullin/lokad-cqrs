using System;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build
{
    public static class ExtendStorageModule
    {

        public static void AtomicIsInAzure(this StorageModule self, string accountId)
        {
            var module = new AzureAtomicStorageModule(accountId);
            self.EnlistModule(module);

        }

        public static void AtomicIsInAzure(this StorageModule self, string accountId, Action<AzureAtomicStorageModule> config)
        {
            var module = new AzureAtomicStorageModule(accountId);
            config(module);
            self.EnlistModule(module);
        }

        public static void StreamingIsInAzure(this StorageModule self, string accountId)
        {
            var module = new AzureStreamingStorage(accountId);
            self.EnlistModule(module);
        }
    }
}