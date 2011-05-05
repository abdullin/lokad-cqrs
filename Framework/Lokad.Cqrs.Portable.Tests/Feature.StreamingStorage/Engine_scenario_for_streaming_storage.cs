using System;
using System.IO;
using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public sealed class Engine_scenario_for_streaming_storage : FiniteEngineScenario
    {
        [DataContract]
        public sealed class Do : Define.Command
        {
            [DataMember]
            public string Id;
        }
        [DataContract]
        public sealed class Finish : Define.Command {}

        public sealed class Consumer : Define.Handle<Do>
        {
            readonly IMessageSender _sender;
            readonly IStorageRoot _root;

            public Consumer(IMessageSender sender, IStorageRoot root)
            {
                _sender = sender;
                _root = root;
            }

            public void Consume(Do message, MessageContext context)
            {
                

                if (string.IsNullOrEmpty(message.Id))
                {
                    var container = _root.GetContainer("test1");
                    container.Create();
                    var data = new byte[42];
                    container.GetItem("data").Write(s => s.Write(data, 0, data.Length));

                    _sender.SendOne(new Do(){Id = "test1"});
                    return;
                }

                using (var mem = new MemoryStream())
                {
                    _root.GetContainer(message.Id).GetItem("data").ReadInto((props, stream) => stream.CopyTo(mem));

                    Assert.AreEqual(42,mem.Position);
                    _sender.SendOne(new Finish(), cb => cb.AddString("finish"));
                }
            }

        }

        protected override void Configure(CqrsEngineBuilder builder)
        {
            builder.Memory(m =>
                {
                    m.AddMemoryProcess("in");
                    m.AddMemorySender("in");
                });
            EnlistMessage(new Do());
        }
    }
}