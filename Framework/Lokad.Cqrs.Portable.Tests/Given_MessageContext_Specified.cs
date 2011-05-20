#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Given_MessageContext_Specified : FiniteEngineScenario
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
            void Consume(TMessage message);
        }

        public sealed class MyMessageA : IMyMessage {}

        public sealed class MyMessageB : IMyMessage {}

        public sealed class MyHandler : IMyHandler<MyMessageA>, IMyHandler<MyMessageB>
        {
            readonly Func<MyContext> _context;

            public MyHandler(Func<MyContext> context)
            {
                _context = context;
            }

            public void Consume(MyMessageA messageA)
            {
                var detail = _context();
                Assert.AreEqual("valid", detail.Token);
                var txt = string.Format("consume A: {0} created on {1}", detail.MessageId, detail.Created);
                Trace.WriteLine(txt);
            }

            public void Consume(MyMessageB messageA)
            {
                var detail = _context();
                Assert.AreEqual("valid", detail.Token);
                var txt = string.Format("Consume B: {0} created on {1}", detail.MessageId, detail.Created);
                Trace.WriteLine(txt);
            }
        }

        [Test]
        public void Test()
        {
            EnlistBatch(new object[]
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
            Enlist((observable, sender, arg3) => observable.OfType<EnvelopeAcked>().Subscribe(e => arg3.Cancel()));
            TestConfiguration(b =>
                {
                    b.Domain(m =>
                        {
                            m.HandlerSample<IMyHandler<IMyMessage>>(c => c.Consume(null));
                            m.ContextFactory(BuildContextOnTheFly);
                        });
                    b.Memory(m =>
                        {
                            m.AddMemorySender("in", cm => cm.IdGeneratorForTests());
                            m.AddMemoryProcess("in", d => d.DispatchAsCommandBatch());
                        });
                });
        }

        static MyContext BuildContextOnTheFly(ImmutableEnvelope envelope, ImmutableMessage item)
        {
            var messageId = string.Format("[{0}]-{1}", envelope.EnvelopeId, item.Index);
            var token = envelope.GetAttribute("token", "");
            return new MyContext(messageId, token, envelope.CreatedOnUtc);
        }
    }
}