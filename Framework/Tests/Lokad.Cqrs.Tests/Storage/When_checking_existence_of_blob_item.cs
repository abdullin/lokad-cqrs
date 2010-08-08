using System;
using System.IO;
using Lokad.Cqrs;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public sealed class When_checking_existence_of_blob_item : StorageItemFixture
	{
		[Test]
		public void Missing_container_returns_false()
		{
			Assert.IsFalse(TestItem.Exists());
		}

		[Test]
		public void Missing_item_returns_false()
		{
			TestContainer.Create();
			Assert.IsFalse(TestItem.Exists());
		}

		[Test]
		public void Failed_condition_returns_false_for_valid_item()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			Assert.IsFalse(TestItem.Exists(StorageCondition.IfMatch("none")));
		}

		[Test]
		public void Returns_true_for_valid_item()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			Assert.IsTrue(TestItem.Exists());
		}

		[Test]
		public void Returns_true_for_valid_item_and_condition()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			Assert.IsTrue(TestItem.Exists(StorageCondition.IfNoneMatch("never")));

		}
	}


}