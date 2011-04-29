#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class When_writing_item_in<TStorage> : StorageItemFixture<TStorage>
        where TStorage : ITestStorage, new()
    {
        [Test]
        public void Missing_container_throws_container_not_found()
        {
            ExpectContainerNotFound(() => Write(TestItem, Guid.Empty));
        }

        [Test]
        public void Missing_container_and_failed_IfMatch_throw_item_not_found()
        {
            ExpectContainerNotFound(() => Write(TestItem, Guid.Empty, StorageCondition.IfMatch("none")));
        }

        [Test]
        public void Missing_item_and_failed_IfMatch_throw_condition_failed()
        {
            TestContainer.Create();
            ExpectConditionFailed(() => Write(TestItem, Guid.Empty, StorageCondition.IfMatch("none")));
        }

        [Test]
        public void Missing_item_and_valid_IfNoneMatch_succeed()
        {
            TestContainer.Create();
            Write(TestItem, Guid.Empty, StorageCondition.IfNoneMatch("none"));
            ShouldHaveGuid(TestItem, Guid.Empty);
        }


        [Test]
        public void Failed_IfMatch_throws_condition_failed()
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