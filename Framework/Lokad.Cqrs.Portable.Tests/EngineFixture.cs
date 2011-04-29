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
using Lokad.Cqrs.Core.Directory.Default;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    public abstract class EngineFixture
    {
        CancellationTokenSource _source;
        CloudEngineHost _host;
        Subject<ISystemEvent> _events;

        protected IObservable<ISystemEvent> Events
        {
            get { return _events; }
        }

        Action<CloudEngineBuilder> _configureForTest;

        Action<CloudEngineBuilder> _configureForFixture = builder => { };

        protected bool TestCompleted { get; private set; }

        protected void CompleteTestAndStopEngine()
        {
            Trace.WriteLine("completing test");
            Trace.Flush();
            TestCompleted = true;
            _source.Cancel();
        }

        IMessageSender Sender
        {
            get { return _host.Resolve<IMessageSender>(); }
        }

        protected void EnlistHandler(Action<string> data)
        {
            _configureForTest += builder => builder.Advanced(cb => cb.RegisterInstance(data));
        }

        protected void EnlistFixtureConfig(Action<CloudEngineBuilder> config)
        {
            _configureForFixture += config;
        }

        protected void SendString(string data)
        {
            Sender.SendOne(new StringCommand {Data = data});
        }

        [SetUp]
        public void SetUp()
        {
            _source = new CancellationTokenSource();
            _events = new Subject<ISystemEvent>();
            _configureForTest = builder => { };
            TestCompleted = false;
        }

        [TearDown]
        public void TearDown()
        {
            _source.Dispose();
            _source = null;

            _host = null;
        }

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

            public void Consume(StringCommand message, MessageContext context)
            {
                _action(message.Data);
            }
        }

        protected void RunEngineTillStopped(Action whenStarted)
        {
            var identifyNested =
                new[] {typeof (EngineFixture), GetType()}
                    .SelectMany(t => t.GetNestedTypes())
                    .Where(t => typeof (IMessage).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .ToArray();

            var builder = new CloudEngineBuilder()
                .EnlistObserver(_events)
                .Domain(d => d
                    .WhereMessages(t => identifyNested.Contains(t))
                    .InCurrentAssembly());

            _configureForFixture(builder);
            _configureForTest(builder);

            using (_host = builder.Build())
            {
                _host.Start(_source.Token);

                whenStarted();

                if (Debugger.IsAttached)
                {
                    _source.Token.WaitHandle.WaitOne();
                }
                else
                {
                    if (!_source.Token.WaitHandle.WaitOne(5000))
                    {
                        Trace.WriteLine("Force cancel");
                        _source.Cancel();
                    }
                }
            }
        }
    }
}