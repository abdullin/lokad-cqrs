using Lokad.Cqrs.Storage;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Tests.Storage
{
	using Here = BlobStorage;
	public sealed class BlobStorage : ITestStorage
	{
		public IStorageContainer GetContainer(string path)
		{
			//var client = CloudStorageAccountStorageClientExtensions.CreateCloudBlobClient(CloudStorageAccount.DevelopmentStorageAccount);
			//var uri = "http://ipv4.fiddler:10000/devstoreaccount1";
			//var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
			var client = CloudStorageAccountStorageClientExtensions.CreateCloudBlobClient(CloudStorageAccount.DevelopmentStorageAccount);
			return new BlobStorageContainer(client.GetBlobDirectoryReference(path), NullLog.Instance);
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
		public sealed class When_writing_blob_item
			: When_writing_item_in<Here>
		{

		}

		[TestFixture]
		public sealed class When_copying_blob_item
			: When_copying_items_in<Here>
		{

		}
		[TestFixture]
		public sealed class When_checking_blob_item
			: When_checking_item_in<Here>
		{
			
		}
	}
}