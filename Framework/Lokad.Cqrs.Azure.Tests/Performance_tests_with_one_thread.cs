using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Autofac;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;
using System.Linq;

namespace Lokad.Cqrs
{
    [TestFixture, Explicit]
    public sealed class Performance_tests_with_one_thread
    {
        [Test]
        public void Test_memory_partition()
        {
            TestConfiguration(c => c.Memory(m =>
                {
                    m.AddMemorySender("test-accelerated");
                    m.AddMemoryProcess("test-accelerated");
                }));
        }

        [Test]
        public void Test_memory_partition_optimized()
        {
            TestConfiguration(c => c.Memory(m =>
                {
                    m.AddMemorySender("test-accelerated");
                    m.AddMemoryProcess("test-accelerated", x => x.DispatcherIsLambda(Factory));
                }));
        }

        [Test]
        public void Test_azure_partition()
        {
            var config = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-accelerated"), config);
            TestConfiguration(c => c.Azure(m =>
                {
                    m.AddAzureSender(config,"test-accelerated");
                    m.AddAzureProcess(config,"test-accelerated");
                }));
        }

        [Test]
        public void Test_File_partition_lambdas()
        {
            var config = FileStorage.CreateConfig("throughput-tests");
            config.Wipe();
            TestConfiguration(c => c.File(m =>
            {
                m.AddFileSender(config, "test-accelerated");
                m.AddFileProcess(config, "test-accelerated", x => x.DispatcherIsLambda(Factory));
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

            Console.WriteLine("{0} messages per second",round);
        }
    }
}