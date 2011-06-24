#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
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

            ExpectConditionFailed(() => TryToRead(TestItem, StreamingCondition.IfMatch("asd")));
        }



        [Test]
        public void Missing_container_and_IfMatch_throw_container_not_found()
        {
            ExpectContainerNotFound(() => TryToRead(TestItem, StreamingCondition.IfMatch("mismatch")));
        }

        [Test]
        public void Missing_container_and_IfNoneMatch_throw_condition_failed()
        {
            ExpectContainerNotFound(() => TryToRead(TestItem, StreamingCondition.IfNoneMatch("mismatch")));
        }

        [Test]
        public void Missing_item_and_IfNoneMatch_throw_item_not_found()
        {
            TestContainer.Create();
            ExpectItemNotFound(() => TryToRead(TestItem, StreamingCondition.IfNoneMatch("mismatch")));
        }

        [Test]
        public void Missing_item_and_IfMatch_throw_item_not_found()
        {
            TestContainer.Create();
            ExpectItemNotFound(() => TryToRead(TestItem, StreamingCondition.IfMatch("mismatch")));
        }

        [Test]
        public void Valid_item_and_valid_IfNoneMatch_exact_return()
        {
            TestContainer.Create();
            var g = Guid.NewGuid();

            Write(TestItem, g);
            ShouldHaveGuid(TestItem, g, StreamingCondition.IfNoneMatch("none"));
        }

        [Test]
        public void Valid_item_and_valid_IfMatch_exact_return()
        {
            TestContainer.Create();
            var g = Guid.NewGuid();

            Write(TestItem, g);

            var tag = TestItem.GetInfo().Value.ETag;
            ShouldHaveGuid(TestItem, g, StreamingCondition.IfMatch(tag));
        }

        [Test]
        public void Valid_item_and_valid_match_wild_return()
        {
            TestContainer.Create();
            var g = Guid.NewGuid();

            Write(TestItem, g);
            ShouldHaveGuid(TestItem, g, StreamingCondition.IfMatch("*"));
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