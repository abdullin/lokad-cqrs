#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Durability;
using Lokad.Cqrs.Durability;
using Lokad.Cqrs.Feature.DefaultInterfaces;
using NUnit.Framework;
using ProtoBuf;
using System.Linq;

namespace Lokad.Cqrs.Tests
{
	[TestFixture]
	public sealed class SmokeTests
	{
		// ReSharper disable InconsistentNaming

		#region Setup/Teardown

		static CloudEngineHost BuildHost()
		{
			var engine = new CloudEngineBuilder();
			engine.DomainIs(m =>
				{
					m.WithDefaultInterfaces();
					m.InCurrentAssembly();
				});

			engine.AddMessageHandler(x =>
				{
					x.ListenToQueue("test-in");
					x.Dispatch<DispatchMessagesToRoute>(r =>
						{
							r.SpecifyRouter(e =>
								{
									if (e.Items.Any(i => i.MappedType == typeof(Hello)))
										return "test-hi";
									if (e.Items.Any(i => i.MappedType == typeof(Bye)))
										return "test-bye";
									return "test-what";
								});
						});
				});

			engine.AddMessageHandler(x =>
				{
					x.ListenToQueue("test-hi", "test-bye");
					x.Dispatch<DispatchEventToMultipleConsumers>();
				});

			engine.AddMessageClient(x => x.DefaultToQueue("test-in"));


			return engine.Build();
		}

		#endregion

		


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
			public void Consume(Bye message)
			{
				Trace.WriteLine("Bye length: " + message.Word.Length);
			}

			public void Consume(Hello message)
			{
				Trace.WriteLine("Hello length: " + message.Word.Length);
			}
		}

		[Test]
		public void Test()
		{
			
			using (var host = BuildHost())
			{
				host.Initialize();

				var client = host.Resolve<IMessageSender>();

				client.Send(new Hello { Word = "World" });
				client.Send(new Hello { Word = new string('1',9000) });
				client.Send(new Bye { Word = "Earth" });

				using (var cts = new CancellationTokenSource())
				{
					var task = host.Start(cts.Token);
					Thread.Sleep(10.Seconds());
					cts.Cancel(true);
					task.Wait(5.Seconds());
				}
				// second run
				using (var cts = new CancellationTokenSource())
				{
					var task = host.Start(cts.Token);
					Thread.Sleep(2.Seconds());
					cts.Cancel(true);
					task.Wait(5.Seconds());
				}

			}
		}

		[Test]
		public void Test2()
		{
		}
	}

	[TestFixture]
	public sealed class FormatTests
	{
		// ReSharper disable InconsistentNaming
		[Test]
		public void Test()
		{
			var r = new Random();
			using (var mem = new MemoryStream())
			{
				var fix = MessageHeader.ForSchema1Data(r.Next(0,1000), r.Next(0, 12));
				Serializer.Serialize(mem, fix);
				Assert.AreEqual(MessageHeader.FixedSize, mem.Position);
			}
		}
	}
}