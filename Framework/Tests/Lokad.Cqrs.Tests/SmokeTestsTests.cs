using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Lokad;
using Lokad.Cqrs;
using Lokad.Cqrs.Default;
using NUnit.Framework;

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
			client.Send(new Hello(){World = "Hello"});
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
		public sealed class doSomething : IConsume<Hello>
		{
			public void Consume(Hello message)
			{
				Trace.WriteLine("Consumed with length: " + message.World.Length);
			}
		}
	}
}