#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Lokad.Cqrs.Feature.AzureStreamingStorage;
using Lokad.Cqrs.Feature.AzureStreamingStorage.Blob;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Tests.Storage
{
	using Here = BlobStorage;

	public sealed class BlobStorage : ITestStorage
	{
		CloudBlobClient _client = GetCustom(); //CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();

		public IStorageContainer GetContainer(string path)
		{
			//UseLocalFiddler();
			return new BlobStorageContainer(_client.GetBlobDirectoryReference(path));
		}

		public static CloudBlobClient GetCustom()
		{
			return CloudStorageAccount.Parse(File.ReadAllText(@"D:\Environment\Azure.blob.test")).CreateCloudBlobClient();
		}

		public StorageWriteOptions GetWriteHints()
		{
			return StorageWriteOptions.None;
		}

		public BlobStorage UseLocalFiddler()
		{
			const string uri = "http://ipv4.fiddler:10000/devstoreaccount1";
			var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
			_client = new CloudBlobClient(uri, credentials);
			return this;
		}

		[TestFixture]
		public sealed class When_checking_blob_item
			: When_checking_item_in<Here>
		{
		}

		[TestFixture]
		public sealed class When_copying_blob_item
			: When_copying_items_in<Here>
		{
		}

		[TestFixture]
		public sealed class When_deleting_blob_item :
			When_deleting_item_in<Here>
		{
		}

		[TestFixture]
		public sealed class When_reading_blob_item :
			When_reading_item_in<Here>
		{
		}


		[TestFixture]
		public sealed class When_reading_blob_item_with_gzip :
			When_reading_item_in<Here>
		{
			public When_reading_blob_item_with_gzip()
			{
				WriteOptions |= StorageWriteOptions.CompressIfPossible;
			}
		}


		[TestFixture]
		public sealed class When_writing_blob_item
			: When_writing_item_in<Here>
		{
		}

		[TestFixture]
		public sealed class When_writing_blob_item_with_gzip
			: When_writing_item_in<Here>
		{
			public When_writing_blob_item_with_gzip()
			{
				WriteOptions |= StorageWriteOptions.CompressIfPossible;
			}
		}
	}
}