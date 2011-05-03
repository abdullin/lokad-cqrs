using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    public abstract class FiniteEngineScenario
    {
        protected bool HandlerFailuresAreExpected { get; set; }
        public readonly IList<object> StartupMessages = new List<object>();


        public void TestConfiguration(Action<CloudEngineBuilder> config)
        {
            var scenario = this;
            var events = new Subject<ISystemEvent>(Scheduler.TaskPool);
            var builder = new CloudEngineBuilder()
                .EnlistObserver(events);

            config(builder);


            var failures = new List<string>();
            var subscriptions = new List<IDisposable>();
            try
            {
                using (var engine = builder.Build())
                using (var t = new CancellationTokenSource())
                {
                    subscriptions.Add(ObservableExtensions.Subscribe<EnvelopeAcked>(events
                            .OfType<EnvelopeAcked>()
                            .Where(ea => ea.Attributes.Any(p => p.Key == "finish")), c => t.Cancel()));

                    subscriptions.Add(events.OfType<EnvelopeAcked>().Where(ea => ea.Attributes.Any(p => p.Key == "fail"))
                        .Subscribe(c =>
                            {
                                failures.Add((string) c.Attributes.First(p => p.Key == "fail").Value);
                                t.Cancel();
                            }));

                    if (!scenario.HandlerFailuresAreExpected)
                    {
                        subscriptions.Add(
                            events.OfType<EnvelopeDispatchFailed>().Subscribe(d =>
                                {
                                    failures.Add(d.Exception.ToString());
                                    t.Cancel();
                                }));
                    }


                    engine.Start(t.Token);

                    foreach (var message in scenario.StartupMessages)
                    {
                        engine.Resolve<IMessageSender>().SendOne(message);
                    }

                    if (Debugger.IsAttached)
                    {
                        t.Token.WaitHandle.WaitOne();
                    }
                    else
                    {
                        t.Token.WaitHandle.WaitOne(5000);
                    }

                    

                    if (failures.Any())
                    {
                        Assert.Fail(failures.First());
                    }

                    if (!t.IsCancellationRequested)
                    {
                        Assert.Fail("Engine should be stopped manually!");
                    }

                }
            }
            finally
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.Dispose();
                }
            }
        }
    }
}