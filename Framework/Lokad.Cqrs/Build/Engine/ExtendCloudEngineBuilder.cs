using System;
using Lokad.Cqrs.Feature.AtomicStorage;

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

        public static void AtomicStorageIsAzure(this StorageModule self, string accountId)
        {
            var module = new AzureAtomicStorageWriterModule(accountId);
            self.EnlistModule(module);

        }
    }

    
}