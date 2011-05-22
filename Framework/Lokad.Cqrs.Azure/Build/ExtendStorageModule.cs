using System;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build
{
    public static class ExtendStorageModule
    {
        //public static void AtomicIsInAzure(Action<DefaultAtomicStorageStrategyBuilder> configure)
        //{
        //    var dictionary = new ConcurrentDictionary<string, byte[]>();
        //    var builder = new DefaultAtomicStorageStrategyBuilder();
        //    configure(builder);
        //    AtomicIs(new MemoryAtomicStorageFactory(dictionary, builder.Build()));
        //}
        //public static void AtomicIsInAzure(this StorageModule self, string accountId)
        //{
        //    AtomicIsInAzure(self, accountId, d => { });
        //}

        public static void AtomicIsInAzure(this StorageModule self, IAzureStorageConfig storage, Action<DefaultAtomicStorageStrategyBuilder> config)
        {
            var builder = new DefaultAtomicStorageStrategyBuilder();
            config(builder);
            AtomicIsInAzure(self, storage, builder.Build());
        }

        public static void AtomicIsInAzure(this StorageModule self, IAzureStorageConfig storage, IAtomicStorageStrategy strategy)
        {
            self.AtomicIs(new AzureAtomicStorageFactory(strategy, storage));
        }

        public static void StreamingIsInAzure(this StorageModule self, IAzureStorageConfig storage)
        {
            self.StreamingIs(new BlobStreamingRoot(storage.CreateBlobClient()));
        }
    }
}