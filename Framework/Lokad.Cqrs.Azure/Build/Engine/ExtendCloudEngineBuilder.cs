using System;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public static class ExtendCloudEngineBuilder
    {
        public static void Azure(this CloudEngineBuilder @this, Action<AzureModule> config)
        {
            var module = new AzureModule();
            config(module);
            @this.EnlistModule(module);
        }

        public static void AtomicIsInAzure(this StorageModule self, string accountId)
        {
            var module = new AzureAtomicStorageWriterModule(accountId);
            self.EnlistModule(module);

        }

        public static void AtomicIsInAzure(this StorageModule self, string accountId, Action<AzureAtomicStorageWriterModule> config)
        {
            var module = new AzureAtomicStorageWriterModule(accountId);
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