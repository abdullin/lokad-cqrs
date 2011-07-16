#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Feature.Dispatch.Directory;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class When_there_are_no_catch_all_handlers : MessageDirectoryFixture
    {
        // ReSharper disable InconsistentNaming
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => mm.Consumer != typeof (ListenToAll));
        }

        [Test]
        public void Orphaned_messages_are_excluded()
        {
            CollectionAssert.DoesNotContain(QueryAllMessageTypes(Map), typeof (NonHandledCommand));
        }
    }
}