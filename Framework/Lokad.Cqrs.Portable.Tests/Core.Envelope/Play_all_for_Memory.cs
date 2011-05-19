using System;
using Lokad.Cqrs.Core.Envelope.Scenarios;
using NUnit.Framework;
using ServiceStack.Text;

namespace Lokad.Cqrs.Core.Envelope
{
    [TestFixture]
    public sealed class Play_all_for_Memory : When_envelope_is_serialized
    {
        protected override ImmutableEnvelope RoundtripViaSerializer(EnvelopeBuilder builder)
        {
            return builder.Build();
        }
    }

    [TestFixture]
    public sealed class EnvelopePrinterTests
    {
        // ReSharper disable InconsistentNaming
        [Test]
        public void Test()
        {
            var b = new EnvelopeBuilder("GUID");
            b.DelayBy(TimeSpan.FromSeconds(10));
            b.AddString("Test");
            b.AddItem(new { Cool = "1"}).AddAttribute("D2","D1");

            Console.WriteLine(b.Build().PrintToString(o => o.Dump()));

        }
    }
}