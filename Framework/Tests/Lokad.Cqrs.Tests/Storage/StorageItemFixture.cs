#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs;
using Lokad.Cqrs.Storage;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public abstract class StorageItemFixture
	{
		protected void Expect<TEx>(Action action) where TEx : StorageBaseException
		{
			// for some reason NUNit exception attribs do not work with Resharpr
			try
			{
				action();
				Assert.Fail("Expected exception '{0}'", typeof (TEx));
			}
			catch (TEx)
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
			//var uri = "http://ipv4.fiddler:10000/devstoreaccount1";
			//var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
			var client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();
			return new BlobStorageContainer(client.GetBlobDirectoryReference(path), NullLog.Instance);
		}

		protected IStorageContainer TestContainer { get; set; }
		protected IStorageItem TestItem { get; set; }

		[SetUp]
		public void SetUp()
		{
			TestContainer = GetContainer("tc-" + Guid.NewGuid().ToString().ToLowerInvariant());
			TestItem = TestContainer.GetItem(Guid.NewGuid().ToString().ToLowerInvariant());
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

		protected void ShouldHaveGuid(IStorageItem storageItem, Guid g, StorageCondition condition = default(StorageCondition))
		{
			bool set = false;

			storageItem.ReadInto((properties, stream) =>
				{
					var b = new byte[16];
					stream.Read(b, 0, 16);
					var actual = new Guid(b);
					Assert.AreEqual(g, actual);

					var props = properties;

					Assert.AreNotEqual(DateTime.MinValue, props.LastModifiedUtc, "Valid date should be present");
					Assert.That(props.ETag, Is.Not.Empty);

					set = true;
				}, condition);
			Assert.IsTrue(set);
		}
	}
}