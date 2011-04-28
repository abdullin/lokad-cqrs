#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.StreamingStorage.Scenarios
{
    public abstract class When_copying_items_in<T> : StorageItemFixture<T> where T : ITestStorage, new()
    {
        [Test]
        public void Test()
        {
            TestContainer.Create();

            var source = GetItem("source");
            var target = GetItem("target");

            Write(source, Guid.Empty);

            target.CopyFrom(source);

            ShouldHaveGuid(target, Guid.Empty);
        }
    }
}