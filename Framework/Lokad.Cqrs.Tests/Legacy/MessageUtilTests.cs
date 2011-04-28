#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using NUnit.Framework;
using ProtoBuf;

namespace Lokad.Cqrs.Legacy
{
    [TestFixture]
    public sealed class MessageUtilTests
    {
        // ReSharper disable InconsistentNaming


        [ProtoContract]
        sealed class MyEvent
        {
            [ProtoMember(1)]
            public string VariableName { get; set; }
        }

        [Test]
        public void StreamingRoundtrips()
        {
        }
    }
}