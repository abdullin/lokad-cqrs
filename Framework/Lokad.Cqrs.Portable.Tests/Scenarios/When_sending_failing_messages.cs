using System;
using System.Linq;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Tests;
using NUnit.Framework;

namespace Lokad.Cqrs.Scenarios
{
    public abstract class When_sending_failing_messages<TEngineProvider> : EngineFixture<TEngineProvider> 
        where TEngineProvider : IConfigureEngineForFixture, new()
    {
        [Test]
        public void Permanent_failure_ends_with_quarnine()
        {
            EnlistHandler(x =>
                {
                    throw new InvalidOperationException("Fail: " + x);
                });

            Events
                .OfType<QuarantinedMessage>().Subscribe(cm =>
                    {
                        Assert.AreEqual("Fail: try", cm.LastException.Message);
                        CompleteTestAndStopEngine();
                    });

            RunEngineTillStopped(() => SendString("try"));

            Assert.IsTrue(TestCompleted);
        }

        [Test]
        public void Transient_failures_are_retried()
        {
            var counter = 0;
            EnlistHandler(x =>
                {
                    if (++counter != 4)
                        throw new InvalidOperationException();
                    CompleteTestAndStopEngine();
                });

            var captured = 0;
            Events
                .Where(t => t is FailedToConsumeMessage).Subscribe(t => captured++);

            RunEngineTillStopped(() => SendString("do"));

            Assert.IsTrue(TestCompleted);
            Assert.AreEqual(3, captured);
        }
    }
}