#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Autofac;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Core.Outbox;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture, Explicit]
    public sealed class Performance_tests_for_throughput
    {
        [Test]
        public void Test_memory_partition()
        {
            TestConfiguration(c => c.Memory(m =>
                {
                    m.AddMemorySender("test-accelerated");
                    m.AddMemoryProcess("test-accelerated", x => x.DispatcherIsLambda(Factory));
                }), 50000);
        }

        [Test]
        public void Test_File_partition()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();
            TestConfiguration(c => c.File(m =>
                {
                    m.AddFileSender(config, "test-accelerated");
                    m.AddFileProcess(config, "test-accelerated", x => x.DispatcherIsLambda(Factory));
                }), 5000);
        }

        static Action<ImmutableEnvelope> Factory(IComponentContext ctx)
        {
            IQueueWriterFactory factory;
            var registry = ctx.Resolve<QueueWriterRegistry>();
            registry.TryGet("memory", out factory);
            var outQueue = factory.GetWriteQueue("out");
            var finishQueue = factory.GetWriteQueue("finish");
            // pass through the envelope
            return envelope =>
                {
                    if (((UsualMessage) envelope.Items[0].Content).IsLast)
                    {
                        finishQueue.PutMessage(envelope);
                    }
                    else
                    {
                        outQueue.PutMessage(envelope);
                    }
                };
        }


        [DataContract]
        public sealed class UsualMessage : Define.Command
        {
            [DataMember(Order = 1)]
            public bool IsLast { get; set; }
        }

        static void TestConfiguration(Action<CqrsEngineBuilder> build, int useMessages)
        {
            var builder = new CqrsEngineBuilder();
            build(builder);


            var subj = new Subject<ISystemEvent>();

            builder.Advanced.Observers.Clear();
            builder.Advanced.RegisterObserver(subj);

            int count = 0;
            var watch = new Stopwatch();
            using (var token = new CancellationTokenSource())
            {
                subj
                    .OfType<EnvelopeAcked>()
                    .Subscribe(ea =>
                        {
                            count += 1;
                            if (ea.Attributes.Any(ia => ia.Key == "last"))
                            {
                                watch.Stop();
                                var messagesPerSecond = count / watch.Elapsed.TotalSeconds;
                                Console.WriteLine(Math.Round(messagesPerSecond, 1));
                                token.Cancel();
                            }
                        });

                using (var engine = builder.Build())
                {
                    // first we send X then we check
                    var sender = engine.Resolve<IMessageSender>();

                    for (int i = 0; i < useMessages; i++)
                    {
                        sender.SendOne(new UsualMessage());
                    }
                    sender.SendOne(new UsualMessage(), b => b.AddString("last"));


                    watch.Start();
                    engine.Start(token.Token);
                    token.Token.WaitHandle.WaitOne(10000);
                }
            }
        }
    }
}