using System;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;
using System.Linq;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class MemoryQuarantineTests : EngineFixture
    {
        // ReSharper disable InconsistentNaming

     
        [Test]
        public void FailureEndsWithQuarantine()
        {
            EnlistHandler(x =>
                {
                    throw new InvalidOperationException("Fail: "+x);
                });

            Events
                .OfType<QuarantinedMessage>()
                .Subscribe(cm =>
                    {
                        Assert.AreEqual("Fail: try", cm.LastException.Message);
                        CompleteTestAndStopEngine();
                    });

            RunEngineTillStopped(() => SendString("try"));

            Assert.IsTrue(TestCompleted);
        }

        [Test]
        public void FewFailuresDoNotQuarantine()
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
                .Where(t => t is FailedToConsumeMessage)
                .Subscribe(t => captured ++);

            RunEngineTillStopped(() => SendString("do"));

            Assert.IsTrue(TestCompleted);
            Assert.AreEqual(3, captured);
        }
    }
}