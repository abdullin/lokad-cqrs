#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public sealed class When_reading_blob_item : StorageItemFixture
	{
		[Test]
		public void Missing_container_throws_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem));
		}

		[Test]
		public void Missing_item_throws_item_not_found()
		{
			TestContainer.Create();

			ExpectItemNotFound(() => TryToRead(TestItem));
			
		}

		[Test]
		public void Failed_condition_throws_condition_failed()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);

			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfMatch("asd")));

			;
		}


		[Test]
		public void Missing_container_and_failed_condition_throw_condition_failed()
		{
			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfMatch("asd")));
		}

		[Test]
		public void Missing_item_and_failed_condition_throw_condition_failed()
		{
			TestContainer.Create();
			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfMatch("asd")));
		}


		[Test]
		public void Valid_item_retrieval_works()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			var storageItem = GetItem("path");

			Write(storageItem, g);
			ShouldBe(g, storageItem, StorageCondition.IfNoneMatch("none"));
		}

		

		static void ShouldBe(Guid guid, IStorageItem item, StorageCondition condition = default(StorageCondition))
		{
			bool set = false;
			item.ReadInto((props, stream) =>
				{
					Guid actual = GetGuid(stream);
					Assert.AreEqual(guid, actual);
					set = true;
				}, condition);
			Assert.IsTrue(set);
		}


		static Guid GetGuid(Stream stream)
		{
			var b = new byte[16];
			stream.Read(b, 0, 16);
			return new Guid(b);
		}
	}
}