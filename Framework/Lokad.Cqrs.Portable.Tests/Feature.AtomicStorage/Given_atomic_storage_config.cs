using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_atomic_storage_config
    {
        // ReSharper disable InconsistentNaming

        public sealed class Message : Define.Command
        {
            
        }

        public sealed class Entity
        {
            public int Count;
        }

        public sealed class Consumer : Define.Handler<Message>
        {
            readonly IMessageSender _sender;
            readonly IAtomicSingletonWriter<Entity> _singleton;

            public Consumer(IMessageSender sender, IAtomicSingletonWriter<Entity> singleton)
            {
                _sender = sender;
                _singleton = singleton;
            }

            public void Consume(Message message, MessageContext context)
            {
                var entity = _singleton.UpdateOrAdd(r => r.Count +=1);
                if (entity.Count == 5)
                {
                    _sender.SendOne(new Message(), cb => cb.AddString("finish",""));
                }
                else
                {
                    _sender.SendOne(new Message());
                }
            }
        }

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
                .Where(ea => ea.Attributes.Any(p => p.Key == "finish")).Subscribe(c => t.Cancel()))
            {
                engine.Start(t.Token);
                engine.Resolve<IMessageSender>().SendOne(new Message());
                t.Token.WaitHandle.WaitOne(5000);

                Assert.IsTrue(t.IsCancellationRequested);
            }
        }


        [Test]
        public void Test()
        {
            TestConfiguration(ceb => ceb.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemoryAtomicStorage();
                    m.AddMemorySender("do");
                }));
        }
        
    }
}