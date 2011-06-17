using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        CloudBlobClient _cloudBlobClient;
        ISingleThreadTapeWriterFactory _writerFactory;
        ITapeReaderFactory _readerFactory;

        protected override void SetUp()
        {
            CloudStorageAccount.SetConfigurationSettingPublisher(
                (configName, configSetter) => configSetter((string) Settings.Default[configName]));

            var cloudStorageAccount = CloudStorageAccount.FromConfigurationSetting("StorageConnectionString");
            _cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            _writerFactory = new BlobTapeWriterFactory(_cloudBlobClient, ContainerName);
            _writerFactory.Init();

            _readerFactory = new BlobTapeReaderFactory(_cloudBlobClient, ContainerName);
        }

        protected override void TearDown()
        {
            _cloudBlobClient.GetContainerReference(ContainerName).Delete();
        }

        protected override TestConfiguration GetConfiguration()
        {
            return new TestConfiguration
            {
                Name = "test",
                WriterFactory = _writerFactory,
                ReaderFactory = _readerFactory
            };
        }
    }
}
