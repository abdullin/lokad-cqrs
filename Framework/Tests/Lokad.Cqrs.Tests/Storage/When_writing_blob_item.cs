using System;
using Lokad.Cqrs;
using Lokad.Quality;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace CloudBus.Tests.Storage
{
	[TestFixture, UsedImplicitly]
	public sealed class When_writing_blob_item : StorageItemFixture
	{
		[Test]
		public void Missing_container_throws_container_not_found()
		{
			ExpectContainerNotFound(() => Write(TestItem, Guid.Empty));
		}

		[Test]
		public void Failed_condition_throws_condition_failed()
		{
			TestContainer.Create();

			ExpectConditionFailed(() => Write(TestItem, Guid.Empty, StorageCondition.IfMatch("tag")));
		}


		[Test]
		public void Unconditional_append_works()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
		}

		[Test]
		public void Conditional_append_works()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty, StorageCondition.IfNoneMatch("tag"));
		}

		[Test]
		public void Unconditional_upsert_works()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			Write(TestItem, Guid.Empty);
		}
	}
}