using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using NUnit.Framework;

namespace Lokad.Cqrs.Tests
{
    public interface IConfigureEngineForFixture
    {
        void Config(CloudEngineBuilder builder);
    }

    public static class EngineFixtureContracts
    {
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
    }

    public abstract class EngineFixture<TEngineProvider>
        where TEngineProvider : IConfigureEngineForFixture, new()
    {
        CancellationTokenSource _source;
        CloudEngineHost _host;
        Subject<ISystemEvent> _events;

        public IObservable<ISystemEvent> Events
        {
            get { return _events; }
        }

        Action<CloudEngineBuilder> _configureEngineForTest;

        

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
            _configureEngineForTest += builder => builder.RegisterInstance(data);
        }

        protected void SendString(string data)
        {
            Sender.Send(new EngineFixtureContracts.StringCommand { Data = data });
        }

        [SetUp]
        public void SetUp()
        {
            _source = new CancellationTokenSource();
            _events = new Subject<ISystemEvent>();
            _configureEngineForTest = builder => { };
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
                new[] { typeof(EngineFixtureContracts), GetType() }
                    .SelectMany(t => t.GetNestedTypes())
                    .Where(t => typeof(IMessage).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .ToArray();

            var builder = new CloudEngineBuilder()
                .RegisterInstance<IObserver<ISystemEvent>>(_events)
                .DomainIs(d => d
                    .WhereMessages(t => identifyNested.Contains(t))
                    .InCurrentAssembly());

            new TEngineProvider().Config(builder);
            _configureEngineForTest(builder);

            using (_host = builder.Build())
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