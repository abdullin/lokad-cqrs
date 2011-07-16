#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Autofac;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture, Explicit]
    public sealed class Performance_tests_for_reaction
    {
        [Test]
        public void Memory_partition_with_polymorphic()
        {
            TestConfiguration(c => c.Memory(m =>
                {
                    m.AddMemorySender("test-accelerated");
                    m.AddMemoryProcess("test-accelerated");
                }));
        }

        [Test]
        public void Memory_partition_with_lambda()
        {
            TestConfiguration(c =>
                c.Memory(m =>
                    {
                        m.AddMemorySender("test-accelerated");
                        m.AddMemoryProcess("test-accelerated", x => x.DispatcherIsLambda(Factory));
                    }));
        }

        [Test]
        public void Azure_partition_with_lambda()
        {
            var config = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("performance"), config);
            TestConfiguration(c => c.Azure(m =>
                {
                    m.AddAzureSender(config, "performance");
                    m.AddAzureProcess(config, "performance", x => x.DispatcherIsLambda(Factory));
                }));
        }

        [Test]
        public void Azure_partition_with_polymorphic()
        {
            var config = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("performance"), config);
            TestConfiguration(c => c.Azure(m =>
                {
                    m.AddAzureSender(config, "performance");
                    m.AddAzureProcess(config, "performance");
                }));
        }


        [Test]
        public void File_partition_with_lambda()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();
            TestConfiguration(c => c.File(m =>
                {
                    m.AddFileSender(config, "test-accelerated");
                    m.AddFileProcess(config, "test-accelerated", x => x.DispatcherIsLambda(Factory));
                }));
        }

        [Test]
        public void File_partition_with_polymorphic()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();
            TestConfiguration(c => c.File(m =>
                {
                    m.AddFileSender(config, "test-accelerated");
                    m.AddFileProcess(config, "test-accelerated");
                }));
        }


        static Action<ImmutableEnvelope> Factory(IComponentContext componentContext)
        {
            var sender = componentContext.Resolve<IMessageSender>();
            return envelope => sender.SendOne(envelope.Items[0].Content);
        }


        // ReSharper disable InconsistentNaming
        [DataContract]
        public sealed class UsualMessage : Define.Command
        {
            [DataMember(Order = 1)]
            public string Word { get; set; }
        }

        public sealed class Handler : Define.Handle<UsualMessage>
        {
            readonly IMessageSender _sender;

            public Handler(IMessageSender sender)
            {
                _sender = sender;
            }

            public void Consume(UsualMessage message)
            {
                _sender.SendOne(message);
            }
        }

        static void TestConfiguration(Action<CqrsEngineBuilder> build)
        {
            var builder = new CqrsEngineBuilder();
            builder.UseProtoBufSerialization();
            build(builder);


            var subj = new Subject<ISystemEvent>();

            builder.Advanced.Observers.Clear();
            builder.Advanced.RegisterObserver(subj);
            int count = 0;
            subj
                .OfType<EnvelopeAcked>()
                .Subscribe(acked => count += 1);

            var watch = new Stopwatch();

            using (var token = new CancellationTokenSource())
            using (var engine = builder.Build())
            {
                engine.Start(token.Token);

                engine
                    .Resolve<IMessageSender>()
                    .SendOne(new UsualMessage());

                watch.Start();
                token.Token.WaitHandle.WaitOne(5000);
            }

            watch.Stop();
            var messagesPerSecond = count / watch.Elapsed.TotalSeconds;
            var round = Math.Round(messagesPerSecond, 1);

            Console.WriteLine("{0} messages per second", round);
        }
    }
}