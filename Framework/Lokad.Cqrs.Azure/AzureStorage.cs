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

        public static NuclearStorage CreateNuclear(IAzureStorageConfig config)
        {
            return CreateNuclear(config, b => { });
        }
        public static NuclearStorage CreateNuclear(IAzureStorageConfig config, IAtomicStorageStrategy strategy)
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

        public static NuclearStorage CreateNuclear(IAzureStorageConfig config, Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var strategyBuilder = new DefaultAtomicStorageStrategyBuilder();
            configStrategy(strategyBuilder);
            var strategy = strategyBuilder.Build();
            return CreateNuclear(config, strategy);
        }

        public static IAzureStorageConfig CreateConfig(CloudStorageAccount account, Action<AzureStorageConfigurationBuilder> configStorage)
        {
            var builder = new AzureStorageConfigurationBuilder(account);
            configStorage(builder);

            return builder.Build();
        }

        public static IAzureStorageConfig CreateConfig(string storageString, Action<AzureStorageConfigurationBuilder> config)
        {
            return CreateConfig(CloudStorageAccount.Parse(storageString), config);
        }

        public static IAzureStorageConfig CreateConfig(string storageString)
        {
            return CreateConfig(storageString, builder => { });
        }

        public static IAzureStorageConfig CreateConfigurationForDev()
        {
            return CreateConfig(CloudStorageAccount.DevelopmentStorageAccount, c => c.Named("azure-dev"));
        }

        public static IAzureStorageConfig CreateConfig(CloudStorageAccount account)
        {
            return CreateConfig(account, builder => { });
        }

        public static IStreamingRoot CreateStreaming(IAzureStorageConfig config)
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