#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs;
using Lokad.Cqrs.Storage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public sealed class When_deleting_blob_item : StorageItemFixture
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
		public void Failed_condition_works()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.Delete(StorageCondition.IfMatch("random"));
		}

		[Test]
		public void Valid_item_with_condition_works()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.Delete(StorageCondition.IfNoneMatch("random"));
		}
	}
}