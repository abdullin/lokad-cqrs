#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Storage;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Lokad.Cqrs.Tests.Storage
{
	public abstract class StorageItemFixture<TStorage>
		where TStorage : ITestStorage, new()
	{
		readonly ITestStorage _factory = new TStorage();

		static void Expect<TEx>(Action action) where TEx : StorageBaseException
		{
			try
			{
				action();
				Assert.Fail("Expected exception '{0}'", typeof (TEx));
			}
			catch (TEx)
			{
			}
		}


		IStorageContainer GetContainer(string path)
		{
			return _factory.GetContainer(path);
		}

		protected IStorageContainer TestContainer { get; private set; }
		protected IStorageItem TestItem { get; private set; }

		[SetUp]
		public void SetUp()
		{
			TestContainer = GetContainer("tc-" + Guid.NewGuid().ToString().ToLowerInvariant());
			TestItem = TestContainer.GetItem(Guid.NewGuid().ToString().ToLowerInvariant());
		}

		[TearDown]
		public void TearDown()
		{
			
			TestContainer.Remove();
			
		}

		protected IStorageItem GetItem(string path)
		{
			return TestContainer.GetItem(path);
		}


		protected void ExpectContainerNotFound(Action action)
		{
			Expect<StorageContainerNotFoundException>(action);
		}

		protected void ExpectItemNotFound(Action action)
		{
			Expect<StorageItemNotFoundException>(action);
		}

		protected void ExpectConditionFailed(Action action)
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
		}

		protected void ShouldHaveGuid(IStorageItem storageItem, Guid g, StorageCondition condition = default(StorageCondition))
		{
			var set = false;

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