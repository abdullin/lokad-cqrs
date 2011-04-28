#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class MemoryConfigurationTests
    {
        // ReSharper disable InconsistentNaming

        #region Domain

        [DataContract]
        public sealed class Message1 : IMessage
        {
            [DataMember] public readonly int Block;

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

            public void Consume(Message1 message, MessageContext context)
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

        #endregion

        static void TestConfiguration(Action<CloudEngineBuilder> config)
        {
            var h = new ManualResetEventSlim();

            var engine = new CloudEngineBuilder()
                .RegisterInstance(h)
                .DomainIs(d => d.WhereMessagesAre<Message1>());

            config(engine);

            using (var eng = engine.Build())
            using (var t = new CancellationTokenSource())
            {
                eng.Start(t.Token);
                eng.Resolve<IMessageSender>().Send(new Message1(0));
                var signaled = h.Wait(TimeSpan.FromSeconds(5), t.Token);
                Assert.IsTrue(signaled);
            }
        }

        [Test]
        public void PartitionWithRouter()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("in");
                    m.AddMemoryRouter("in", me => "memory:do");
                    m.AddMemoryProcess("do");
                }));
        }

        [Test]
        public void Direct()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("in");
                    m.AddMemoryProcess("in");
                }));
        }

        [Test]
        public void RouterChain()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("in");
                    m.AddMemoryRouter("in",
                        me => (((Message1) me.Items[0].Content).Block%2) == 0 ? "memory:do1" : "memory:do2");
                    m.AddMemoryProcess("do1");
                    m.AddMemoryProcess("do2");
                }));
        }
    }
}