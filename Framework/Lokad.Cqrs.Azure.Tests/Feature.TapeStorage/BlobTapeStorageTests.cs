using System;
using Lokad.Cqrs.Properties;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class BlobTapeStorageTests : TapeStorageTests
    {
        const string ContainerName = "blob-tape-test";

        CloudStorageAccount _cloudStorageAccount;
        ISingleThreadTapeWriterFactory _writerFactory;
        ITapeReaderFactory _readerFactory;

        protected override void PrepareEnvironment()
        {
            CloudStorageAccount.SetConfigurationSettingPublisher(
                (configName, configSetter) => configSetter((string) Settings.Default[configName]));

            _cloudStorageAccount = CloudStorageAccount.FromConfigurationSetting("StorageConnectionString");
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();

            try
            {
                cloudBlobClient.GetContainerReference(ContainerName).FetchAttributes();
                throw new InvalidOperationException("Container '" + ContainerName + "' already exists!");
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode != StorageErrorCode.ResourceNotFound)
                    throw new InvalidOperationException("Container '" + ContainerName + "' already exists!");
            }
        }

        protected override Factories GetTapeStorageInterfaces()
        {
            var config = AzureStorage.CreateConfig(_cloudStorageAccount);
            _writerFactory = new SingleThreadBlobTapeWriterFactory(config, ContainerName);
            _writerFactory.Initialize();

            _readerFactory = new BlobTapeReaderFactory(config, ContainerName);

            const string name = "test";

            return new Factories
            {
                Writer = _writerFactory.GetOrCreateWriter(name),
                Reader = _readerFactory.GetReader(name)
            };
        }

        protected override void FreeResources()
        {
            _writerFactory = null;
            _readerFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();
            cloudBlobClient.GetContainerReference(ContainerName).Delete();
        }
    }
}
