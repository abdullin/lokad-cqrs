#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

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

        public sealed class AtomicMessage : Define.Command {}

        public sealed class Entity
        {
            public int Count;
        }

        public sealed class Consumer : Define.Handler<AtomicMessage>
        {
            readonly IMessageSender _sender;
            readonly IAtomicSingletonWriter<Entity> _singleton;

            public Consumer(IMessageSender sender, IAtomicSingletonWriter<Entity> singleton)
            {
                _sender = sender;
                _singleton = singleton;
            }

            public void Consume(AtomicMessage atomicMessage, MessageContext context)
            {
                var entity = _singleton.AddOrUpdate(r => r.Count += 1);
                if (entity.Count == 5)
                {
                    _sender.SendOne(new AtomicMessage(), cb => cb.AddString("finish", ""));
                }
                else
                {
                    _sender.SendOne(new AtomicMessage());
                }
            }
        }

        public sealed class NuclearMessage : Define.Command {}

        public sealed class NuclearHandler : Define.Handler<NuclearMessage>
        {
            readonly IMessageSender _sender;
            readonly NuclearStorage _storage;

            public NuclearHandler(IMessageSender sender, NuclearStorage storage)
            {
                _sender = sender;
                _storage = storage;
            }

            public void Consume(NuclearMessage atomicMessage, MessageContext context)
            {
                var result = _storage
                    .AddOrUpdateSingleton<Entity>(s => s.Count += 1);

                if (result.Count == 5)
                {
                    _sender.SendOne(new AtomicMessage(), cb => cb.AddString("finish", ""));
                }
                else
                {
                    _sender.SendOne(new AtomicMessage());
                }
            }
        }

        static void TestConfiguration(Action<CloudEngineBuilder> config, params Define.Command[] commands)
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
                engine.Resolve<IMessageSender>().SendBatch(commands);
                t.Token.WaitHandle.WaitOne(5000);

                Assert.IsTrue(t.IsCancellationRequested);
            }
        }


        [Test]
        public void When_atomic_config_is_requested()
        {
            TestConfiguration(ceb => ceb.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemoryAtomicStorage();
                    m.AddMemorySender("do");
                }), new AtomicMessage());
        }

        [Test]
        public void When_nuclear_config_is_requested()
        {
            TestConfiguration(ceb => ceb.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemoryAtomicStorage();
                    m.AddMemorySender("do");
                }), new NuclearMessage());
        }
    }
}