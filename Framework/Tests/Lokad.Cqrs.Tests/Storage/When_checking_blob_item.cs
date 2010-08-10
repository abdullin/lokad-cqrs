#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs;
using Lokad.Cqrs.Storage;
using Lokad.Testing;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public sealed class When_checking_of_blob_item : StorageItemFixture
	{
		[Test]
		public void Missing_container_returns_false()
		{
			TestItem.GetInfo().ShouldFail();
		}

		[Test]
		public void Missing_item_returns_false()
		{
			TestContainer.Create();
			TestItem.GetInfo().ShouldFail();
		}

		[Test]
		public void Failed_condition_returns_false_for_valid_item()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.GetInfo(StorageCondition.IfMatch("mismatch")).ShouldFail();
		}

		[Test]
		public void Returns_true_for_valid_item()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.GetInfo().ShouldPass();
		}

		[Test]
		public void Returns_true_for_valid_item_and_condition()
		{
			TestContainer.Create();
			Write(TestItem, Guid.Empty);
			TestItem.GetInfo(StorageCondition.IfNoneMatch("never")).ShouldPass();
		}
	}
}