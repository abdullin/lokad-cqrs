using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;
using System.Linq;

namespace Lokad.Cqrs
{
    [TestFixture]
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
            build(builder);


            var subj = new Subject<ISystemEvent>();
            builder.Observers.Clear();
            builder.Observer(subj);
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
                token.Token.WaitHandle.WaitOne(10000);
            }

            watch.Stop();
            var messagesPerSecond = count / watch.Elapsed.TotalSeconds;
            Console.WriteLine(Math.Round(messagesPerSecond, 1));
        }
    }
}