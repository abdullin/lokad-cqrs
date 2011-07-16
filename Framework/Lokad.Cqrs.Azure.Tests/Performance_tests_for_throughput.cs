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
    public sealed class Performance_throughput_tests
    {
        [Test]
        public void Memory_lambda()
        {
            TestConfiguration(c => c.Memory(m =>
                {
                    m.AddMemorySender("test");
                    m.AddMemoryProcess("test", x => x.DispatcherIsLambda(Factory));
                }), 1000000);
        }

        [Test]
        public void Memory_polymorphic()
        {
            TestConfiguration(c => c.Memory(m =>
            {
                m.AddMemorySender("test");
                m.AddMemoryProcess("test");
            }), 100000);
        }

        [Test]
        public void Throughput_Azure_polymorphic()
        {
            var config = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("throughput"), config);
            TestConfiguration(c => c.Azure(m =>
            {
                m.AddAzureSender(config, "throughput");
                m.AddAzureProcess(config, "throughput");
            }), 100);
        }

        [Test]
        public void Throughput_Azure_lambda()
        {
            var config = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("throughput"), config);
            TestConfiguration(c => c.Azure(m =>
            {
                m.AddAzureSender(config, "throughput");
                m.AddAzureProcess(config, "throughput", x => x.DispatcherIsLambda(Factory));
            }), 100);
        }

        [Test]
        public void File_lambda()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();
            TestConfiguration(c => c.File(m =>
                {
                    m.AddFileSender(config, "test");
                    m.AddFileProcess(config, "test", x => x.DispatcherIsLambda(Factory));
                }), 10000);
        }

        [Test]
        public void File_polymorphic()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();
            TestConfiguration(c => c.File(m =>
            {
                m.AddFileSender(config, "test");
                m.AddFileProcess(config, "test");
            }), 10000);
        }


        static Action<ImmutableEnvelope> Factory(IComponentContext ctx)
        {
            // pass through the envelope
            return envelope =>
                {
                    var msg = ((UsualMessage) (envelope.Items[0].Content));
                    if (!msg.HasData)
                        throw new InvalidOperationException("Data should be present to check for deserialization");
                };
        }


        [DataContract]
        public sealed class UsualMessage : Define.Command
        {
            [DataMember(Order = 1)]
            public bool HasData { get; set; }
        }

        static void TestConfiguration(Action<CqrsEngineBuilder> build, int useMessages)
        {
            var builder = new CqrsEngineBuilder();
            build(builder);


            var subj = new Subject<ISystemEvent>();

            builder.Advanced.Observers.Clear();
            builder.Advanced.RegisterObserver(subj);

            var step = (useMessages / 5);
            int count = 0;
            var watch = new Stopwatch();
            using (var token = new CancellationTokenSource())
            {
                subj
                    .OfType<EnvelopeAcked>()
                    .Subscribe(ea =>
                        {
                            count += 1;

                            if ((count % step) == 0)
                            {
                                var messagesPerSecond = count / watch.Elapsed.TotalSeconds;
                                Console.WriteLine("{0} - {1}", count, Math.Round(messagesPerSecond, 1));
                            }


                            if (ea.Attributes.Any(ia => ia.Key == "last"))
                            {
                                token.Cancel();
                            }
                        });

                using (var engine = builder.Build())
                {
                    // first we send X then we check
                    var sender = engine.Resolve<IMessageSender>();

                    for (int i = 0; i < useMessages; i++)
                    {
                        sender.SendOne(new UsualMessage { HasData = true });
                    }
                    sender.SendOne(new UsualMessage { HasData = true}, b => b.AddString("last"));


                    watch.Start();
                    engine.Start(token.Token);
                    token.Token.WaitHandle.WaitOne(10000);
                }
            }
        }
    }
}