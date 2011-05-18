using System;
using NUnit.Framework;
using System.Linq;

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class When_listing_items_in<TStorage> :
        StorageItemFixture<TStorage> where TStorage : ITestStorage, new()
    {
        [Test]
        public void Missing_container_throws_error()
        {
            ExpectContainerNotFound(() => TestContainer.ListItems());
        }

        [Test]
        public void Existing_empty_container_returns_empty()
        {
            TestContainer.Create();
            CollectionAssert.IsEmpty(TestContainer.ListItems());
        }

        [Test]
        public void Existing_container_lists_items()
        {
            TestContainer.Create();
            var newGuid = Guid.NewGuid();
            Write(TestItem, newGuid);
            var first = TestContainer.ListItems().First();

            ShouldHaveGuid(TestContainer.GetItem(first),newGuid);
        }
    
        
    }
}