using System;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs
{
    public static class AzureStorage
    {
        public static NuclearStorage CreateNuclear(CloudStorageAccount account)
        {
            return CreateNuclear(account, b => { });
        }

        public static NuclearStorage CreateNuclear(IAzureStorageConfiguration config)
        {
            return CreateNuclear(config, b => { });
        }
        public static NuclearStorage CreateNuclear(IAzureStorageConfiguration config, IAzureAtomicStorageStrategy strategy)
        {
            var factory = new AzureAtomicStorageFactory(strategy, config, new ImmediateTracingObserver());
            factory.Initialize();
            return new NuclearStorage(factory);
        }
        public static NuclearStorage CreateNuclear(CloudStorageAccount account, Action<DefaultAzureAtomicStorageStrategyBuilder> configStrategy)
        {
            var config = new AzureStorageConfigurationBuilder(account, "default").Build();
            return CreateNuclear(config, configStrategy);
        }

        public static NuclearStorage CreateNuclear(IAzureStorageConfiguration config, Action<DefaultAzureAtomicStorageStrategyBuilder> configStrategy)
        {
            var strategyBuilder = new DefaultAzureAtomicStorageStrategyBuilder();
            configStrategy(strategyBuilder);
            var strategy = strategyBuilder.Build();
            return CreateNuclear(config, strategy);
        }

        public static IStreamingRoot CreateStreaming(IAzureStorageConfiguration config)
        {
            return new BlobStreamingRoot(config.CreateBlobClient());
        }

        public static IStreamingRoot CreateStreaming(CloudStorageAccount config)
        {
            var account = new AzureStorageConfigurationBuilder(config, "default");
            return CreateStreaming(account.Build());
        }
    }
}