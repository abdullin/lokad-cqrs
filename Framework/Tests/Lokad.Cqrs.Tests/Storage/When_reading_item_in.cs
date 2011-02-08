#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Lokad.Cqrs.Storage;
using Lokad.Storage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Tests.Storage
{
	public abstract class When_reading_item_in<TStorage> : 
		StorageItemFixture<TStorage> where TStorage : ITestStorage, new()
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
		public void Valid_item_and_failed_IfMatch_throws_condition_failed()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);

			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfMatch("asd")));
		}

		[Test]
		public void Valid_item_and_failed_IfUnmodifiedSince_throws_condition_failed()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfUnmodifiedSince(DateTime.MinValue)));
		}

		[Test]
		public void Valid_item_and_failed_IfModifiedSince_throw_condition()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			var info = TestItem.GetInfo().Value.LastModifiedUtc;

			Thread.Sleep(1.Seconds());
			ExpectConditionFailed(() => TryToRead(TestItem, StorageCondition.IfModifiedSince(info)));
		}


		[Test]
		public void Missing_container_and_IfMatch_throw_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfMatch("mismatch")));
		}

		[Test]
		public void Missing_container_and_IfNoneMatch_throw_condition_failed()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfNoneMatch("mismatch")));
		}

		[Test]
		public void Missing_container_and_IfUnmodifiedSince_throw_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfUnmodifiedSince(DateTime.MinValue)));
		}

		[Test]
		public void Missing_container_and_IfModifiedSince_throw_container_not_found()
		{
			ExpectContainerNotFound(() => TryToRead(TestItem, StorageCondition.IfModifiedSince(DateTime.MinValue)));
		}


		[Test]
		public void Missing_item_and_IfUnmodifiedSince_throw_item_not_found()
		{
			TestContainer.Create();
			ExpectItemNotFound(() => TryToRead(TestItem, StorageCondition.IfUnmodifiedSince(DateTime.MinValue)));
		}

		[Test]
		public void Missing_item_and_IfNoneMatch_throw_item_not_found()
		{
			TestContainer.Create();
			ExpectItemNotFound(() => TryToRead(TestItem, StorageCondition.IfNoneMatch("mismatch")));
		}

		[Test]
		public void Missing_item_and_IfMatch_throw_item_not_found()
		{
			TestContainer.Create();
			ExpectItemNotFound(() => TryToRead(TestItem, StorageCondition.IfMatch("mismatch")));
		}

		[Test]
		public void Valid_item_and_valid_IfNoneMatch_exact_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			Write(TestItem, g);
			ShouldHaveGuid(TestItem, g, StorageCondition.IfNoneMatch("none"));
		}

		[Test]
		public void Valid_item_and_valid_IfMatch_exact_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			Write(TestItem, g);

			var tag = TestItem.GetInfo().Value.ETag;
			ShouldHaveGuid(TestItem, g, StorageCondition.IfMatch(tag));
		}

		[Test]
		public void Valid_item_and_valid_match_wild_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();

			Write(TestItem, g);
			ShouldHaveGuid(TestItem, g, StorageCondition.IfMatch("*"));
		}

		[Test]
		public void Valid_item_and_valid_IfUnmodifiedSince_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();
			Write(TestItem, g);
			var tag = TestItem.GetInfo().Value.LastModifiedUtc;
			ShouldHaveGuid(TestItem, g, StorageCondition.IfUnmodifiedSince(tag));
		}


		[Test]
		public void Valid_item_and_valid_IfModifiedSince_return()
		{
			TestContainer.Create();
			var g = Guid.NewGuid();
			Write(TestItem, g);
			var tag = TestItem.GetInfo().Value.LastModifiedUtc;
			ShouldHaveGuid(TestItem, g, StorageCondition.IfModifiedSince(tag.AddDays(-1)));
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