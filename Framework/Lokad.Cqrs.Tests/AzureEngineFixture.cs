using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using NUnit.Framework;

namespace Lokad.Cqrs.Tests
{
    public abstract class AzureEngineFixture
    {
        CancellationTokenSource _source;
        CloudEngineHost _host;
        Subject<ISystemEvent> _events;

        public IObservable<ISystemEvent> Events
        {
            get { return _events; }
        }

        Action<CloudEngineBuilder> _whenConfiguring;

        [DataContract]
        public sealed class StringCommand : Define.Command
        {
            [DataMember]
            public string Data { get; set; }
        }

        public sealed class StringHandler : Define.Handler<StringCommand>
        {
            readonly Action<string> _action;


            public StringHandler(Action<string> action)
            {
                _action = action;
            }

            public void Consume(StringCommand message)
            {
                _action(message.Data);
            }
        }

        public bool TestCompleted { get; set; }

        public void CompleteTestAndStopEngine()
        {
            Trace.WriteLine("completing test");
            TestCompleted = true;
            _source.Cancel();
        }

        protected IMessageSender Sender
        {
            get { return _host.Resolve<IMessageSender>(); }
        }

        protected void EnlistHandler(Action<string> data)
        {
            _whenConfiguring += builder => builder.RegisterInstance(data);
        }

        protected void SendString(string data)
        {
            Sender.Send(new StringCommand { Data = data });
        }

        [SetUp]
        public void SetUp()
        {
            _source = new CancellationTokenSource();
            _events = new Subject<ISystemEvent>();
            _whenConfiguring = builder => { };
            TestCompleted = false;
        }

        [TearDown]
        public void TearDown()
        {
            _source.Dispose();
            _source = null;

            _host = null;
        }

        protected void RunEngineTillStopped(Action whenStarted)
        {
            var identifyNested =
                new[] { typeof(AzureEngineFixture), GetType() }
                    .SelectMany(t => t.GetNestedTypes())
                    .Where(t => typeof(IMessage).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .ToArray();


            var engine = new CloudEngineBuilder()
                .RegisterInstance<IObserver<ISystemEvent>>(_events)
                .AddMessageClient("default:inda")
                .Azure(m => m.AddPartition(p =>
                    {
                        p.QueueVisibilityTimeout(50);
                        p.WhenFactoryCreated(c => c.SetupForTesting());
                    }, "inda"))
                .DomainIs(d => d.WhereMessages(t => identifyNested.Contains(t)).InCurrentAssembly());

            _whenConfiguring(engine);

            using (_host = engine.Build())
            {
                _host.Start(_source.Token);

                whenStarted();

                if (!_source.Token.WaitHandle.WaitOne(5000))
                {
                    Trace.WriteLine("Force cancel");
                    _source.Cancel();
                }
            }
        }

    }
}