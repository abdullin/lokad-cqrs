#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

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
						m.ListenTo("test-hi", "test-bye");
						m.WithMultipleConsumers();
					})
				.PublishSubscribe(m =>
					{
						m.ListenTo("test-in");
						m.ManagerIsInMemory()
							.DirectBinding("Hello", "test-hi")
							.DirectBinding("Bye", "test-bye");
					})
				.SendMessages(m => m.DefaultToQueue("test-in"))
				.Build();
		}

		[Test]
		public void Test()
		{
			Host.Initialize();
			Host.Start();
			var client = Host.Resolve<IMessageClient>();

			client.Send(new Hello {Word = "World"});
			client.Send(new Hello {Word = Rand.String.NextText(6000, 6000)});
			client.Send(new Bye {Word = "Earth"});

			SystemUtil.Sleep(50.Seconds());
			Host.Stop();
		}

		[Test]
		public void Test2()
		{
		}


		[DataContract]
		public sealed class Hello : IMessage
		{
			[DataMember(Order = 1)]
			public string Word { get; set; }
		}

		[DataContract]
		public sealed class Bye : IMessage
		{
			[DataMember(Order = 1)]
			public string Word { get; set; }
		}

		public sealed class DoSomething : IConsume<Hello>, IConsume<Bye>
		{
			public void Consume(Hello message)
			{
				Trace.WriteLine("Hello length: " + message.Word.Length);
				Trace.WriteLine("Message total: " + MessageContext.Current.Header.GetTotalLength());
			}

			public void Consume(Bye message)
			{
				Trace.WriteLine("Bye length: " + message.Word.Length);
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
				var fix = MessageHeader.ForData(Rand.Next(1000), Rand.Next(0, 12), 0);
				Serializer.Serialize(mem, fix);
				Assert.AreEqual(MessageHeader.FixedSize, mem.Position);
			}
		}
	}
}