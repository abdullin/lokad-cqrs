#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Feature.StreamingStorage.Scenarios;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;
using Lokad.Cqrs.Build.Engine;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public sealed class Play_all_for_BlobStreaming : ITestStorage
    {
        CloudBlobClient _client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();

        public IStreamingContainer GetContainer(string path)
        {
            //UseLocalFiddler();
            return new BlobStreamingContainer(_client.GetBlobDirectoryReference(path));
        }

        public static CloudBlobClient GetCustom()
        {
            return
                CloudStorageAccount.Parse(File.ReadAllText(@"D:\Environment\Azure.blob.test")).CreateCloudBlobClient();
        }

        public StreamingWriteOptions GetWriteHints()
        {
            return StreamingWriteOptions.None;
        }

        public Play_all_for_BlobStreaming UseLocalFiddler()
        {
            const string uri = "http://ipv4.fiddler:10000/devstoreaccount1";
            var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
            _client = new CloudBlobClient(uri, credentials);
            return this;
        }

        [TestFixture]
        public sealed class When_checking_blob_item
            : When_checking_item_in<Play_all_for_BlobStreaming>
        {
        }

        [TestFixture]
        public sealed class When_copying_blob_item
            : When_copying_items_in<Play_all_for_BlobStreaming>
        {
        }

        [TestFixture]
        public sealed class When_deleting_blob_item :
            When_deleting_item_in<Play_all_for_BlobStreaming>
        {
        }

        [TestFixture]
        public sealed class When_reading_blob_item :
            When_reading_item_in<Play_all_for_BlobStreaming>
        {
        }


        [TestFixture]
        public sealed class When_reading_blob_item_with_gzip :
            When_reading_item_in<Play_all_for_BlobStreaming>
        {
            public When_reading_blob_item_with_gzip()
            {
                WriteOptions |= StreamingWriteOptions.CompressIfPossible;
            }
        }


        [TestFixture]
        public sealed class When_writing_blob_item
            : When_writing_item_in<Play_all_for_BlobStreaming>
        {
        }

        [TestFixture]
        public sealed class When_writing_blob_item_with_gzip
            : When_writing_item_in<Play_all_for_BlobStreaming>
        {
            public When_writing_blob_item_with_gzip()
            {
                WriteOptions |= StreamingWriteOptions.CompressIfPossible;
            }
        }

        [TestFixture]
        public sealed class When_configured_in_engine
        {
            [Test]
            public void Test()
            {
                new Engine_scenario_for_streaming_storage().TestConfiguration(cb =>
                {
                    cb.Azure(m =>
                        {
                            m.AddAzureAccount("azure-dev", CloudStorageAccount.DevelopmentStorageAccount);
                            m.WipeAccountsAtStartUp = true;
                        });

                    cb.Storage(m => m.StreamingIsInAzure("azure-dev"));
                });
            }
        }

    }
}