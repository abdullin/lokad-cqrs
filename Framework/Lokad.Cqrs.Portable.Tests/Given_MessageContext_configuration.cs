#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Given_MessageContext_configuration
    {
        public sealed class MyContext
        {
            public readonly string MessageId;
            public readonly string Token;
            public readonly DateTimeOffset Created;

            public MyContext(string messageId, string token, DateTimeOffset created)
            {
                MessageId = messageId;
                Token = token;
                Created = created;
            }
        }

        public interface IMyMessage {}

        public interface IMyHandler<TMessage> where TMessage : IMyMessage
        {
            void Consume(TMessage message, MyContext detail);
        }

        public sealed class MyMessageA : IMyMessage {}

        public sealed class MyMessageB : IMyMessage {}

        public sealed class MyHandler : IMyHandler<MyMessageA>, IMyHandler<MyMessageB>
        {
            public void Consume(MyMessageA messageA, MyContext detail)
            {
                Assert.AreEqual("valid", detail.Token);
                var txt = string.Format("consume A: {0} created on {1}", detail.MessageId, detail.Created);
                Trace.WriteLine(txt);
            }

            public void Consume(MyMessageB messageA, MyContext detail)
            {
                Assert.AreEqual("valid", detail.Token);
                var txt = string.Format("Consume B: {0} created on {1}", detail.MessageId, detail.Created);
                Trace.WriteLine(txt);
            }
        }


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var builder = new CqrsEngineBuilder();
            builder.Domain(m =>
                {
                    m.HandlerSample<IMyHandler<IMyMessage>>(c => c.Consume(null, null));
                    m.ContextFactory(BuildContextOnTheFly);
                });
            builder.Memory(m =>
                {
                    m.AddMemorySender("in", cm => cm.IdGeneratorForTests());
                    m.AddMemoryProcess("in", d => d.Dispatch<DispatchCommandBatch>());
                });

            var observer = new Subject<ISystemEvent>();
            builder.EnlistObserver(observer);
            _events = observer;
            _engine = builder.Build();
        }

        static MyContext BuildContextOnTheFly(ImmutableEnvelope envelope, ImmutableMessage item)
        {
            var messageId = string.Format("[{0}]-{1}", envelope.EnvelopeId, item.Index);
            var token = envelope.GetAttribute("token", "");
            return new MyContext(messageId, token, envelope.CreatedOnUtc);
        }

        CqrsEngineHost _engine;
        IObservable<ISystemEvent> _events;


        [Test]
        public void Test()
        {
            using (var t = new CancellationTokenSource())
            using (_events
                .OfType<EnvelopeAcked>()
                .Subscribe(a => t.Cancel()))
            {
                _engine.Start(t.Token);
                var sender = _engine.Resolve<IMessageSender>();

                sender.SendBatch(new object[]
                    {
                        new MyMessageA(),
                        new MyMessageB()
                    },
                    cb =>
                        {
                            cb.AddString("token", "valid");
                            cb.AddString("aggregate", "customer-1");
                        }
                    );
                t.Token.WaitHandle.WaitOne(10000);

                Assert.IsTrue(t.IsCancellationRequested);
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _engine.Dispose();
        }

        // ReSharper disable InconsistentNaming
    }
}