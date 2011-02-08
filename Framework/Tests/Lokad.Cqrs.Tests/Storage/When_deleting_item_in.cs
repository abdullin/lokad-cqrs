using System;
using Lokad.Cqrs.Storage;
using Lokad.Storage;
using Lokad.Testing;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Tests.Storage
{
	public abstract class When_deleting_item_in<TStorage> : 
		StorageItemFixture<TStorage> where TStorage : ITestStorage, new()
	{
		[Test]
		public void Missing_container_throws_container_not_found()
		{
			ExpectContainerNotFound(() => TestItem.Delete());
		}

		[Test]
		public void Missing_item_works()
		{
			TestContainer.Create();
			TestItem.Delete();
		}

		[Test]
		public void Missing_item_and_failed_IfMatch_work()
		{
			TestContainer.Create();
			TestItem.Delete(StorageCondition.IfMatch("some"));
		}

		[Test]
		public void Valid_item_and_failed_IfMatch_dont_delete()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.Delete(StorageCondition.IfMatch("random"));
			TestItem.GetInfo().ShouldPass();
		}

		[Test]
		public void Valid_item_and_valid_IfMatch_wild_delete()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.Delete(StorageCondition.IfMatch("*"));
			TestItem.GetInfo().ShouldFail();
		}

		[Test]
		public void Valid_item_and_valid_IfNoneMatch_delete()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.Delete(StorageCondition.IfNoneMatch("random"));
			TestItem.GetInfo().ShouldFail();
		}
	}
}