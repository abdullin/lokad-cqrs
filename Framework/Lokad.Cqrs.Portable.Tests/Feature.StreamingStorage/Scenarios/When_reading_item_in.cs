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
        public void Valid_item_and_failed_IfUnmodifiedSince_throws_condition_failed()
        {
            TestContainer.Create();
            Write(TestItem, Guid.Empty);
            ExpectConditionFailed(() => TryToRead(TestItem, StreamingCondition.IfUnmodifiedSince(DateTime.MinValue)));
        }

        [Test]
        public void Valid_item_and_failed_IfModifiedSince_throw_condition()
        {
            TestContainer.Create();
            Write(TestItem, Guid.Empty);
            var info = TestItem.GetInfo().Value.LastModifiedUtc;

            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            ExpectConditionFailed(() => TryToRead(TestItem, StreamingCondition.IfModifiedSince(info)));
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
        public void Missing_container_and_IfUnmodifiedSince_throw_container_not_found()
        {
            ExpectContainerNotFound(() => TryToRead(TestItem, StreamingCondition.IfUnmodifiedSince(DateTime.MinValue)));
        }

        [Test]
        public void Missing_container_and_IfModifiedSince_throw_container_not_found()
        {
            ExpectContainerNotFound(() => TryToRead(TestItem, StreamingCondition.IfModifiedSince(DateTime.MinValue)));
        }


        [Test]
        public void Missing_item_and_IfUnmodifiedSince_throw_item_not_found()
        {
            TestContainer.Create();
            ExpectItemNotFound(() => TryToRead(TestItem, StreamingCondition.IfUnmodifiedSince(DateTime.MinValue)));
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
        public void Valid_item_and_valid_IfUnmodifiedSince_return()
        {
            TestContainer.Create();
            var g = Guid.NewGuid();
            Write(TestItem, g);
            var tag = TestItem.GetInfo().Value.LastModifiedUtc;
            ShouldHaveGuid(TestItem, g, StreamingCondition.IfUnmodifiedSince(tag));
        }


        [Test]
        public void Valid_item_and_valid_IfModifiedSince_return()
        {
            TestContainer.Create();
            var g = Guid.NewGuid();
            Write(TestItem, g);
            var tag = TestItem.GetInfo().Value.LastModifiedUtc;
            ShouldHaveGuid(TestItem, g, StreamingCondition.IfModifiedSince(tag.AddDays(-1)));
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