using System;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class MemoryQuarantineTests
    {
        // ReSharper disable InconsistentNaming

        #region Domain

        [DataContract]
        public sealed class Command : Define.Command
        {
            [DataMember]
            public readonly int Block;

            public Command(int block)
            {
                Block = block;
            }
        }

        public sealed class Handler : Define.Handler<Command>
        {
            readonly ManualResetEventSlim _slim;
            readonly MemoryQuarantineTests _tests;

            public Handler(ManualResetEventSlim slim, MemoryQuarantineTests tests)
            {
                _slim = slim;
                _tests = tests;
            }

            public void Consume(Command message)
            {
                if (_tests.Counter == 4)
                {
                    _slim.Set();
                }
                else
                {
                    _tests.Counter += 1;
                    throw new InvalidOperationException();
                }
                
            }
        }

        

        #endregion

        public int Counter = 0;

        void TestConfiguration(Action<CloudEngineBuilder> config)
        {
            var h = new ManualResetEventSlim();

            var engine = new CloudEngineBuilder()
                .RegisterInstance(h)
                .RegisterInstance(this)
                .DomainIs(d => d.WhereMessagesAre<Command>());

            config(engine);

            using (var eng = engine.Build())
            using (var t = new CancellationTokenSource())
            {
                eng.Start(t.Token);
                eng.Resolve<IMessageSender>().Send(new Command(0));
                var signaled = h.Wait(TimeSpan.FromSeconds(5), t.Token);
                Assert.IsTrue(signaled);
            }
        }

        [Test]
        public void SimpleRetry()
        {
            TestConfiguration(x => x
                .AddMessageClient("memory:do")
                .AddMemoryPartition("do"));
        }

        [Test]
        public void Direct()
        {
            TestConfiguration(x => x
                .AddMessageClient("memory:in")
                .AddMemoryPartition("in"));
        }

        [Test]
        public void RouterChain()
        {
            TestConfiguration(x => x
                .AddMessageClient("memory:in")
                .AddMemoryRouter("in",
                    me => (((Command)me.Items[0].Content).Block % 2) == 0 ? "memory:do1" : "memory:do2")
                .AddMemoryPartition("do1")
                .AddMemoryPartition("do2"));
        }
    }
}