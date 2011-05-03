#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_finite_engine_scenario
    {
        // ReSharper disable InconsistentNaming

        static void TestConfiguration(IFiniteEngineScenario scenario)
        {
            var events = new Subject<ISystemEvent>(Scheduler.TaskPool);
            var builder = new CloudEngineBuilder()
                .EnlistObserver(events);

            scenario.Configure(builder);


            var failures = new List<string>();
            var subscriptions = new List<IDisposable>();
            try
            {
                using (var engine = builder.Build())
                using (var t = new CancellationTokenSource())
                {
                    subscriptions.Add(events
                        .OfType<EnvelopeAcked>()
                        .Where(ea => ea.Attributes.Any(p => p.Key == "finish"))
                        .Subscribe(c => t.Cancel()));

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

        [Test]
        public void When_atomic_config_is_requested()
        {
            TestConfiguration(new Engine_scenario_for_AtomicStorage_in_partition());
        }

        [Test]
        public void When_nuclear_config_is_requested()
        {
            TestConfiguration(new Engine_scenario_for_NuclearStorage_in_partition());
        }

        [Test]
        public void When_custom_view_domain()
        {
            TestConfiguration(new Engine_scenario_for_custom_view_domain());
        }
    }
}