#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Given_MessageContext_Omitted : FiniteEngineScenario
    {
        interface IMyMessage {}

        interface IMyHandler<in TMessage> where TMessage : IMyMessage
        {
            void Consume(TMessage message);
        }
        [DataContract]
        public sealed class MyMessageA : IMyMessage {}

        public sealed class MyHandler : IMyHandler<MyMessageA>
        {
            readonly IMessageSender _sender;

            public MyHandler(IMessageSender sender)
            {
                _sender = sender;
            }

            public void Consume(MyMessageA messageA)
            {
                _sender.SendOne(messageA, cb => cb.AddString("finish"));
            }
        }


        [Test]
        public void Test()
        {
            EnlistMessage(new MyMessageA());
            TestConfiguration(b =>
                {
                    b.Domain(m => m.HandlerSample<IMyHandler<IMyMessage>>(c => c.Consume(null)));
                    b.Memory(m =>
                    {
                        m.AddMemorySender("in", cm => cm.IdGeneratorForTests());
                        m.AddMemoryProcess("in", d => d.DispatchAsCommandBatch());
                    });
                });
        }
    }
}