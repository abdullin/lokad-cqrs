#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Serialization;
using NUnit.Framework;
using ProtoBuf;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Durability
{
    [TestFixture]
    public sealed class LegacyCompatibilityTests
    {
        [Test]
        public void Works()
        {
            const string msg =
                @"DV17zncRvgAAAAAAAAAZYwAAAAAAAAAlAAAAAAohCAciHUFjdGl2YXRlQWRkaXRpb25hbFVzZXJDb21tYW5kCiEIASIdQWN0aXZhdGVBZGRpdGlvbmFsVXNlckNvbW1hbmQKPggIIjpodHRwczovL3NhbGVzY2FzdC5xdWV1ZS5jb3JlLndpbmRvd3MubmV0L3NhbGVzY2FzdC1wdWJsaXNoCigICSIkMWIzNzhjMzUtZGJhNi00MWNlLWFkOTEtOWU5NDAwZTQ2ZDY2CgwICijlo/rIkcjo5kgI8gISMGh1YjovL3VzZXJzLzY3MzMwY2Q5LTBlYzYtNDBlOS05NDNjLTllOTQwMGRlYzY3NBosZGJRbWVUZlcwaUlaMjE3eTJZZzRxR1JsRG5FNDRXUGNha3NqcENIeEl1Yz0=";

            var bytes = Convert.FromBase64String(msg);


            var streamer = new EnvelopeStreamer(new EnvelopeSerializerWithProtoBuf(),
                DataSerializerWithProtoBuf.For<ActivateUserCommand>());

            var obj = streamer.ReadDataMessage(bytes);
            Assert.IsInstanceOfType(typeof (ActivateUserCommand), obj.Items[0].Content);
        }

        [ProtoContract(Name = "ActivateAdditionalUserCommand")]
        public sealed class ActivateUserCommand
        {
            [ProtoMember(1)] public long UserId;
            [ProtoMember(2)] public string LoginIdentity;
            [ProtoMember(3)] public string RegistrationToken;
        }
    }
}