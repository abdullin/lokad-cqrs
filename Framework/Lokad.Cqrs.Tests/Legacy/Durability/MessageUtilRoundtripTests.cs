#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Envelope;
using NUnit.Framework;

namespace Lokad.Cqrs.Legacy.Durability
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public sealed class MessageUtilRoundtripTests
    {
        [Test]
        public void EmptyRoundtrip()
        {
            var builder = new MessageEnvelopeBuilder("my-id").Build();
            var bytes = TestSerializer.Streamer.SaveDataMessage(builder);
            var envelope = TestSerializer.Streamer.ReadDataMessage(bytes);
            Assert.AreEqual(envelope.EnvelopeId, "my-id");
        }

        [Test]
        public void RoundTripWithPayload()
        {
            var builder = new MessageEnvelopeBuilder("my-id");
            builder.Attributes["Custom"] = 1;
            builder.Attributes[EnvelopeAttributes.CreatedUtc] = DateTimeOffset.UtcNow;

            builder.AddItem(new MyMessage(42));


            var bytes = TestSerializer.Streamer.SaveDataMessage(builder.Build());
            var envelope = TestSerializer.Streamer.ReadDataMessage(bytes);
            Assert.AreEqual(1, envelope.GetAttribute("Custom"));
            Assert.AreEqual(1, envelope.Items.Length);
            Assert.AreEqual(42, ((MyMessage) envelope.Items[0].Content).Value);
        }

        [Serializable]
        sealed class MyMessage
        {
            public readonly int Value;

            public MyMessage(int value)
            {
                Value = value;
            }
        }
    }
}