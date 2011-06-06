using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
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
            readonly IStreamingRoot _root;

            public Consumer(IMessageSender sender, IStreamingRoot root)
            {
                _sender = sender;
                _root = root;
            }

            public void Consume(Do message)
            {
                

                if (string.IsNullOrEmpty(message.Id))
                {
                    var container = _root.GetContainer("test1");
                    container.Create();
                    container.GetItem("data").Write(s =>
                        {
                            using (var writer = new StreamWriter(s,Encoding.UTF8))
                            {
                                writer.Write(new string('*',50000));
                            }
                        });

                    _sender.SendOne(new Do {Id = "test1"});
                    return;
                }
                string result = null;
                _root.GetContainer(message.Id).GetItem("data").ReadInto((props, stream) =>
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                            }
                        });
                Assert.AreEqual(new string('*', 50000), result);
                _sender.SendOne(new Finish(), cb => cb.AddString("finish"));
            }

        }

        protected override void Configure(CqrsEngineBuilder b)
        {
            b.Memory(m =>
                {
                    m.AddMemoryProcess("in");
                    m.AddMemorySender("in");
                });
            EnlistMessage(new Do());
        }
    }
}