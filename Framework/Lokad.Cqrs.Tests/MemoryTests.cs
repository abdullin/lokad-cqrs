using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.DefaultInterfaces;
using NUnit.Framework;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs
{
	[TestFixture]
	public sealed class MemoryTests
	{
		// ReSharper disable InconsistentNaming
		[DataContract]
		public sealed class Message1 : IMessage
		{
			[DataMember]
			public readonly int Block;

			public Message1(int block)
			{
				Block = block;
			}
		}

		public sealed class Consumer : IConsume<Message1>
		{
			readonly IMessageSender _sender;
			readonly ManualResetEventSlim _slim;

			public Consumer(IMessageSender sender, ManualResetEventSlim slim)
			{
				_sender = sender;
				_slim = slim;
			}

			public void Consume(Message1 message)
			{
				if (message.Block < 5)
				{
					_sender.Send(new Message1(message.Block + 1));
				}
				else
				{
					_slim.Set();
				}
			}
		}

		[Test]
		public void Test()
		{
			var h = new ManualResetEventSlim();

			var engine = new CloudEngineBuilder()
					.AddMessageClient("memory:in")
					.AddMemoryRouter("in", me => "memory:do")
					.AddMemoryPartition("do")
					.RegisterInstance(h)
					.DomainIs(d =>
						{
							d.InCurrentAssembly();
							d.WithDefaultInterfaces();
							d.WhereMessagesAre<Message1>();
						});

			using (var eng = engine.Build())
			using (var t = new CancellationTokenSource())
			{
				eng.Start(t.Token);
				eng.Resolve<IMessageSender>().Send(new Message1(0));
				var signaled = h.Wait(5.Seconds(), t.Token);
				Assert.IsTrue(signaled);
			}
		}
	}
}