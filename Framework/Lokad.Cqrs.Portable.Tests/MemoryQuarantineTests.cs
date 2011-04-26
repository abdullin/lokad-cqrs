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
            HandleString(x =>
                {
                    throw new InvalidOperationException();
                });

            Events
                .OfType<QuarantinedMessage>()
                .Subscribe(cm => StopAndComplete());

            RunEngineTillStopped(() => SendString("do"));

            Assert.IsTrue(TestCompleted);
        }

        [Test]
        public void FewFailuresDoNotQuarantine()
        {
            var counter = 0;
            HandleString(x =>
                {
                    if (++counter != 4)
                        throw new InvalidOperationException();
                    StopAndComplete();
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