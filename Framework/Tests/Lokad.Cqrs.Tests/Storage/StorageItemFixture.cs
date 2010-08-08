using System;
using System.IO;
using Lokad.Cqrs;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;
using CloudStorageAccountStorageClientExtensions = Microsoft.WindowsAzure.StorageClient.CloudStorageAccountStorageClientExtensions;

namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public abstract class StorageItemFixture
	{
		protected StorageItemFixture()
		{
			

		}

		protected void Expect<TEx>(Action action) where TEx : StorageBaseException
		{
			// for some reason NUNit exception attribs do not work with Resharpr
			try
			{
				action();
				Assert.Fail("Expected exception '{0}'", typeof(TEx));
			}
			catch(TEx)
			{
				
			}
			//catch(Exception ex)
			//{
			//    var message = string.Format("Expected exception '{0}' but got '{1}", typeof (TEx).Name, ex.GetType().Name);
			//    throw new AssertionException(message, ex);
			//}


		}

		protected IStorageContainer GetContainer(string path)
		{
			//var client = CloudStorageAccountStorageClientExtensions.CreateCloudBlobClient(CloudStorageAccount.DevelopmentStorageAccount);

			//var uri = "http://ipv4.fiddler:10000";
			//var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
			//var client = new CloudBlobClient(uri, credentials);
			var client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();

			
			return new BlobStorageContainer(client.GetBlobDirectoryReference(path), NullLog.Instance);
		}

		protected IStorageContainer TestContainer { get; set; }
		protected IStorageItem TestItem { get; set; }

		[SetUp]
		public void SetUp()
		{
			TestContainer = GetContainer("test");
			TestItem = TestContainer.GetItem("test");
		}

		[TearDown]
		public void TearDown()
		{
			TestContainer.Delete();
		}

		protected IStorageItem GetItem(string path)
		{
			return TestContainer.GetItem(path);
		}

		

		public void ExpectContainerNotFound(Action action) 
		{
			Expect<StorageContainerNotFoundException>(action);
		}

		public void ExpectItemNotFound(Action action)
		{
			Expect<StorageItemNotFoundException>(action);
		}

		public void ExpectConditionFailed(Action action)
		{
			Expect<StorageConditionFailedException>(action);
		}

		protected void Write(IStorageItem storageItem, Guid g, StorageCondition condition = default(StorageCondition))
		{
			storageItem.Write(stream => stream.Write(g.ToByteArray(), 0, 16), condition);


		}

		

		protected void TryToRead(IStorageItem item, StorageCondition condition = default(StorageCondition))
		{
			item.ReadInto((props, stream) => stream.Read(new byte[1], 0, 1), condition);
			Assert.Fail("Should not get here");
		}

	}
}