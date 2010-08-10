#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad;
using Lokad.Cqrs;
using Lokad.Cqrs.Storage;
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
		public void Valid_item_and_failed_match_throws_condition_failed()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);

			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfMatch("asd")));
		}

		[Test]
		public void Valid_item_and_failed_unmodified_throws_condition_failed()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfUnmodifiedSince(DateTime.MinValue)));
		}

		[Test] //Ignore("Seems to be problem in Azure, GET is executed instead of not-modified")
		public void Valid_item_and_failed_modified_throw_condition()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			var info = TestItem.GetInfo().Value.LastModifiedUtc;

			SystemUtil.Sleep(1.Seconds());
			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfModifiedSince(info)));
		}


		[Test] //, Ignore("Seems to be problem in Azure, returns precondition failure instead")
		public void Missing_container_and_match_throw_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfMatch("mismatch")));
		}

		[Test]
		public void Missing_container_and_nonematch_throw_condition_failed()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfNoneMatch("mismatch")));
		}

		[Test]
		public void Missing_container_and_unmodified_throw_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfUnmodifiedSince(DateTime.MinValue)));
		}

		[Test]
		public void Missing_container_and_modified_throw_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfModifiedSince(DateTime.MinValue)));
		}


		[Test]
		public void Missing_item_and_unmodified_throw_item_not_found()
		{
			TestContainer.Create();
			ExpectItemNotFound(() => TryToRead(TestItem, StorageCondition.IfUnmodifiedSince(DateTime.MinValue)));
		}

		[Test]
		public void Missing_item_and_nonematch_throw_item_not_found()
		{
			TestContainer.Create();
			ExpectItemNotFound(() => TryToRead(TestItem, StorageCondition.IfNoneMatch("mismatch")));
		}

		[Test] //Ignore("Seems to be problem in Azure, returns precondition failure instead")
		public void Missing_item_and_match_throw_item_not_found()
		{
			TestContainer.Create();
			ExpectItemNotFound(() => TryToRead(TestItem, StorageCondition.IfMatch("mismatch")));
		}

		[Test]
		public void Valid_item_and_valid_nonematch_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			Write(TestItem, g);
			ShouldHaveGuid(TestItem, g, StorageCondition.IfNoneMatch("none"));
		}

		[Test]
		public void Valid_item_and_valid_match_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			Write(TestItem, g);

			var tag = TestItem.GetInfo().Value.ETag;
			ShouldHaveGuid(TestItem, g, StorageCondition.IfMatch(tag));
		}

		[Test]
		public void Valid_item_and_valid_unmodified_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();
			Write(TestItem, g);
			var tag = TestItem.GetInfo().Value.LastModifiedUtc;
			ShouldHaveGuid(TestItem, g, StorageCondition.IfUnmodifiedSince(tag));
		}


		[Test]
		public void Valid_item_and_valid_modified_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();
			Write(TestItem, g);
			var tag = TestItem.GetInfo().Value.LastModifiedUtc;
			ShouldHaveGuid(TestItem, g, StorageCondition.IfUnmodifiedSince(tag.AddDays(-1)));
		}

		[Test]
		public void Valid_item_returns()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			Write(TestItem, g);
			ShouldHaveGuid(TestItem, g);
		}
	}
}