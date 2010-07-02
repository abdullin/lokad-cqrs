using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Lokad;
using Lokad.Cqrs;
using Lokad.Cqrs.Default;
using Lokad.Cqrs.Queue;
using NUnit.Framework;
using ProtoBuf;

namespace CloudBus.Tests
{
	[TestFixture]
	public sealed class SmokeTestsTests
	{
		// ReSharper disable InconsistentNaming

		ICloudEngineHost Host { get; set; }

		[SetUp]
		public void SetUp()
		{
			Host = new CloudEngineBuilder()
				.Domain(m =>
				{
					m.WithDefaultInterfaces();
					m.UseProtocolBuffers();
					m.InCurrentAssembly();
				})
				.HandleMessages(m =>
				{
					m.ListenTo("test-01");
					m.WithMultipleConsumers();
				}).SendMessages(m => m.DefaultToQueue("test-01")).Build();
		}

		[Test]
		public void Test()
		{
			Host.Initialize();
			Host.Start();
			var client = Host.Resolve<IMessageClient>();

			client.Send(new Hello {World = "Hello"});
			client.Send(new Hello{World = Rand.String.NextText(6000,6000)});

			SystemUtil.Sleep(50.Seconds());
			Host.Stop();
		}


		[DataContract]
		public sealed class Hello : IMessage
		{
			[DataMember(Order = 1)]
			public string World { get; set; }
		}
		public sealed class DoSomething : IConsume<Hello>
		{
			public void Consume(Hello message)
			{
				Trace.WriteLine("Consumed with length: " + message.World.Length);

				Trace.WriteLine("Message total: " + MessageContext.Current.Header.GetTotalLength());
			}
		}
	}

	[TestFixture]
	public sealed class FormatTests
	{
		// ReSharper disable InconsistentNaming
		[Test]
		public void Test()
		{
			
			using (var mem = new MemoryStream())
			{
				var fix = MessageHeader.ForData(Rand.Next(1000),Rand.Next(0,12),0);
				Serializer.Serialize(mem,fix);
				Assert.AreEqual(MessageHeader.FixedSize, mem.Position);
			}
		}
	}
}