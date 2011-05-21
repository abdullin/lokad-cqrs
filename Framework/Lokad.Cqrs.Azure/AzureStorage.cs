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
        public static NuclearStorage CreateNuclear(IAzureStorageConfiguration config, IAtomicStorageStrategy strategy)
        {
            var factory = new AzureAtomicStorageFactory(strategy, config);
            factory.Initialize();
            return new NuclearStorage(factory);
        }
        public static NuclearStorage CreateNuclear(CloudStorageAccount account, Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var config = new AzureStorageConfigurationBuilder(account).Build();
            return CreateNuclear(config, configStrategy);
        }

        public static NuclearStorage CreateNuclear(IAzureStorageConfiguration config, Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var strategyBuilder = new DefaultAtomicStorageStrategyBuilder();
            configStrategy(strategyBuilder);
            var strategy = strategyBuilder.Build();
            return CreateNuclear(config, strategy);
        }

        public static IAzureStorageConfiguration CreateConfiguration(CloudStorageAccount account, Action<AzureStorageConfigurationBuilder> configStorage)
        {
            var builder = new AzureStorageConfigurationBuilder(account);
            configStorage(builder);

            return builder.Build();
        }

        public static IAzureStorageConfiguration CreateConfigurationForDev()
        {
            return CreateConfiguration(CloudStorageAccount.DevelopmentStorageAccount, c => c.Named("azure-dev"));
        }

        public static IAzureStorageConfiguration CreateConfiguration(CloudStorageAccount account)
        {
            return CreateConfiguration(account, builder => { });
        }

        public static IStreamingRoot CreateStreaming(IAzureStorageConfiguration config)
        {
            return new BlobStreamingRoot(config.CreateBlobClient());
        }

        public static IStreamingRoot CreateStreaming(CloudStorageAccount config)
        {
            var account = new AzureStorageConfigurationBuilder(config);
            return CreateStreaming(account.Build());
        }
    }
}