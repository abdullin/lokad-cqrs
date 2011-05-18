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
            AtomicIsInAzure(self, accountId, d => { });
        }

        public static void AtomicIsInAzure(this StorageModule self, string accountId, Action<DefaultAzureAtomicStorageStrategyBuilder> config)
        {
            var builder = new DefaultAzureAtomicStorageStrategyBuilder();
            config(builder);
            AtomicIsInAzure(self, accountId, builder.Build());
        }

        public static void AtomicIsInAzure(this StorageModule self, string accountId, IAzureAtomicStorageStrategy strategy)
        {
            var module = new AzureAtomicStorageModule(accountId, strategy);
            self.EnlistModule(module);
        }



        public static void StreamingIsInAzure(this StorageModule self, string accountId)
        {
            var module = new AzureStreamingStorageModule(accountId);
            self.EnlistModule(module);
        }
    }
}