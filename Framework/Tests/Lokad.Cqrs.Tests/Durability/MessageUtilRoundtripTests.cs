using System;
using Lokad.Cqrs.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.Durability
{
	[TestFixture]
	public sealed class MessageUtilRoundtripTests
	{
		// ReSharper disable InconsistentNaming

		[Test]
		public void Test()
		{
			var builder = new MessageEnvelopeBuilder(Guid.NewGuid());
			var bytes = MessageUtil.SaveDataMessage(builder, TestSerializer.Instance);

			var envelope = MessageUtil.ReadMessage(bytes, TestSerializer.Instance);
		}

		
		
	}
}