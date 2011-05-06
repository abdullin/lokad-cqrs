#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class StorageItemFixture<TStorage>
        where TStorage : ITestStorage, new()
    {
        readonly ITestStorage _factory = new TStorage();


        static void Expect<TEx>(Action action) where TEx : StreamingBaseException
        {
            try
            {
                action();
                Assert.Fail("Expected exception '{0}', but got nothing", typeof (TEx));
            }
            catch (TEx)
            {
            }
        }


        IStreamingContainer GetContainer(string path)
        {
            return _factory.GetContainer(path);
        }

        protected IStreamingContainer TestContainer { get; private set; }
        protected IStreamingItem TestItem { get; private set; }

        protected StreamingWriteOptions WriteOptions { get; set; }

        [SetUp]
        public void SetUp()
        {
            TestContainer = GetContainer("tc-" + Guid.NewGuid().ToString().ToLowerInvariant());
            TestItem = TestContainer.GetItem(Guid.NewGuid().ToString().ToLowerInvariant());
        }

        [TearDown]
        public void TearDown()
        {
            TestContainer.Delete();
        }

        protected IStreamingItem GetItem(string path)
        {
            return TestContainer.GetItem(path);
        }


        protected void ExpectContainerNotFound(Action action)
        {
            Expect<StreamingContainerNotFoundException>(action);
        }

        protected void ExpectItemNotFound(Action action)
        {
            Expect<StreamingItemNotFoundException>(action);
        }

        protected void ExpectConditionFailed(Action action)
        {
            Expect<StreamingConditionFailedException>(action);
        }

        protected void Write(IStreamingItem streamingItem, Guid g, StreamingCondition condition = default(StreamingCondition))
        {
            streamingItem.Write(stream => stream.Write(g.ToByteArray(), 0, 16), condition, WriteOptions);
        }


        protected void TryToRead(IStreamingItem item, StreamingCondition condition = default(StreamingCondition))
        {
            item.ReadInto((props, stream) => stream.Read(new byte[1], 0, 1), condition);
        }

        protected void ShouldHaveGuid(IStreamingItem streamingItem, Guid g,
            StreamingCondition condition = default(StreamingCondition))
        {
            var set = false;
            Guid actual = Guid.Empty;
            StreamingItemInfo streamingItemInfo = null;
            streamingItem.ReadInto((properties, stream) =>
                {
                    var b = new byte[16];
                    stream.Read(b, 0, 16);
                    actual = new Guid(b);
                    set = true;
                    streamingItemInfo = properties;
                }, condition);

            Assert.AreEqual(g, actual);


            Assert.AreNotEqual(DateTime.MinValue, streamingItemInfo.LastModifiedUtc, "Valid date should be present");
            Assert.That(streamingItemInfo.ETag, Is.Not.Empty);

            set = true;

            Assert.IsTrue(set);
        }
    }
}