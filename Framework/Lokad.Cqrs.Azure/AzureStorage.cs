using System;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;
using Microsoft.WindowsAzure;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs
{

    /// <summary>
    /// Helper class to access Azure storage outside of the engine, if needed
    /// </summary>
    public static class AzureStorage
    {

        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage, using the default
        /// storage configuration and atomic strategy.
        /// </summary>
        /// <param name="storageAccount">The storage account.</param>
        /// <returns>new instance of the nuclear storage</returns>
        public static NuclearStorage CreateNuclear(CloudStorageAccount storageAccount)
        {
            return CreateNuclear(storageAccount, b => { });
        }

        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage, using the default storage
        /// strategy.
        /// </summary>
        /// <param name="storageConfig">The storage config.</param>
        /// <returns>new instance of the nuclear storage</returns>
        public static NuclearStorage CreateNuclear(IAzureStorageConfig storageConfig)
        {
            return CreateNuclear(storageConfig, b => { });
        }

        /// <summary> Creates the simplified nuclear storage wrapper around Atomic storage. </summary>
        /// <param name="storageConfig">The storage config.</param>
        /// <param name="strategy">The atomic storage strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(IAzureStorageConfig storageConfig, IAtomicStorageStrategy strategy)
        {
            var factory = new AzureAtomicStorageFactory(strategy, storageConfig);
            factory.Initialize();
            return new NuclearStorage(factory);
        }
        /// <summary> Creates the simplified nuclear storage wrapper around Atomic storage. </summary>
        /// <param name="storageAccount">The storage account.</param>
        /// <param name="configStrategy">The config strategy builder.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(CloudStorageAccount storageAccount, Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var config = new AzureStorageConfigurationBuilder(storageAccount).Build();
            return CreateNuclear(config, configStrategy);
        }

        /// <summary> Creates the simplified nuclear storage wrapper around Atomic storage. </summary>
        /// <param name="storageConfig">The storage config.</param>
        /// <param name="configStrategy">The config strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(IAzureStorageConfig storageConfig, Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var strategyBuilder = new DefaultAtomicStorageStrategyBuilder();
            configStrategy(strategyBuilder);
            var strategy = strategyBuilder.Build();
            return CreateNuclear(storageConfig, strategy);
        }

        /// <summary> Creates the storage access configuration. </summary>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        /// <param name="storageConfigurationStorage">The config storage.</param>
        /// <returns></returns>
        public static IAzureStorageConfig CreateConfig(CloudStorageAccount cloudStorageAccount, Action<AzureStorageConfigurationBuilder> storageConfigurationStorage)
        {
            var builder = new AzureStorageConfigurationBuilder(cloudStorageAccount);
            storageConfigurationStorage(builder);

            return builder.Build();
        }

        /// <summary>
        /// Creates the storage access configuration.
        /// </summary>
        /// <param name="storageString">The storage string.</param>
        /// <param name="storageConfiguration">The storage configuration.</param>
        /// <returns></returns>
        public static IAzureStorageConfig CreateConfig(string storageString, Action<AzureStorageConfigurationBuilder> storageConfiguration)
        {
            return CreateConfig(CloudStorageAccount.Parse(storageString), storageConfiguration);
        }

        /// Creates the storage access configuration.
        /// <param name="storageString">The storage string.</param>
        /// <returns></returns>
        public static IAzureStorageConfig CreateConfig(string storageString)
        {
            return CreateConfig(storageString, builder => { });
        }

        /// <summary>
        /// Creates the storage access configuration for the development storage emulator.
        /// </summary>
        /// <returns></returns>
        public static IAzureStorageConfig CreateConfigurationForDev()
        {
            return CreateConfig(CloudStorageAccount.DevelopmentStorageAccount, c => c.Named("azure-dev"));
        }

        /// <summary>
        /// Creates the storage access configuration.
        /// </summary>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        /// <returns></returns>
        public static IAzureStorageConfig CreateConfig(CloudStorageAccount cloudStorageAccount)
        {
            return CreateConfig(cloudStorageAccount, builder => { });
        }

        /// <summary>
        /// Creates the streaming storage out of the provided storage config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static IStreamingRoot CreateStreaming(IAzureStorageConfig config)
        {
            return new BlobStreamingRoot(config.CreateBlobClient());
        }

        /// <summary>
        /// Creates the streaming storage out of the provided cloud storage account.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static IStreamingRoot CreateStreaming(CloudStorageAccount config)
        {
            var account = new AzureStorageConfigurationBuilder(config);
            return CreateStreaming(account.Build());
        }
    }
}