#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class MemoryConfigurationTests
    {
        // ReSharper disable InconsistentNaming

        #region Domain
        [DataContract]
        public sealed class Message1 : Define.Command
        {
            public readonly int Block;

            public Message1(int block)
            {
                Block = block;
            }
        }

        public sealed class Consumer : Define.Handle<Message1>
        {
            readonly IMessageSender _sender;

            public Consumer(IMessageSender sender)
            {
                _sender = sender;
            }

            public void Consume(Message1 atomicMessage, MessageContext context)
            {
                if (atomicMessage.Block < 5)
                {
                    _sender.SendOne(new Message1(atomicMessage.Block + 1));
                }
            }
        }

        #endregion

        static void TestConfiguration(Action<CloudEngineBuilder> config)
        {
            var events = new Subject<ISystemEvent>(Scheduler.TaskPool);
            var builder = new CloudEngineBuilder()
                .EnlistObserver(events);

            config(builder);

            using (var engine = builder.Build())
            using (var t = new CancellationTokenSource())
            using (events
                .OfType<EnvelopeAcked>()
                .Where(ea => ea.QueueName == "do")
                .Skip(5)
                .Subscribe(c => t.Cancel()))
            {
                engine.Start(t.Token);
                engine.Resolve<IMessageSender>().SendOne(new Message1(0));
                t.Token.WaitHandle.WaitOne(5000);

                Assert.IsTrue(t.IsCancellationRequested);
            }
        }

        [Test]
        public void PartitionWithRouter()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("in", module => module.IdGeneratorForTests());
                    m.AddMemoryRouter("in", me => "memory:do");
                    m.AddMemoryProcess("do");
                }));
        }

        [Test]
        public void Direct()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("do", module => module.IdGeneratorForTests());
                    m.AddMemoryProcess("do");
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
                    m.AddMemoryRouter(new[]{"do1", "do2"}, me => "memory:do");
                    m.AddMemoryProcess("do");
                }));
        }
    }
}