#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Tests;
using NUnit.Framework;

namespace Lokad.Cqrs.Durability
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
			var bytes = MessageUtil.SaveDataMessage(builder, TestSerializer.Instance);
			var envelope = MessageUtil.ReadDataMessage(bytes, TestSerializer.Instance);
			Assert.AreEqual(envelope.EnvelopeId, "my-id");
		}

		[Test]
		public void RoundTripWithPayload()
		{
			var builder = new MessageEnvelopeBuilder("my-id");
			builder.Attributes["Custom"] = 1;
			builder.Attributes[MessageAttributes.Envelope.CreatedUtc] = DateTime.UtcNow;

			builder.AddItem(new MyMessage(42));


			var bytes = MessageUtil.SaveDataMessage(builder.Build(), TestSerializer.Instance);
			var envelope = MessageUtil.ReadDataMessage(bytes, TestSerializer.Instance);
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