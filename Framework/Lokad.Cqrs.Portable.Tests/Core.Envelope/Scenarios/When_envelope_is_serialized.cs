using System;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Envelope
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable DoNotCallOverridableMethodsInConstructor

    public abstract class When_envelope_is_serialized
    {
        protected abstract IEnvelopeSerializer GetSerializer();
        readonly EnvelopeStreamer CurrentStreamer;

        protected MessageEnvelope RoundtripViaSerializer(MessageEnvelopeBuilder builder)
        {
            var envelope = builder.Build();
            var bytes = CurrentStreamer.SaveDataMessage(envelope);
            return CurrentStreamer.ReadDataMessage(bytes);
        }

        protected When_envelope_is_serialized()
        {

            var serializer = GetSerializer();
            CurrentStreamer = new EnvelopeStreamer(serializer, new DataSerializerWithBinaryFormatter());
        }

        [Test]
        public void Empty_roundtrip_should_work()
        {
            var builder = new MessageEnvelopeBuilder("my-id");
            var envelope = RoundtripViaSerializer(builder);
            Assert.AreEqual(envelope.EnvelopeId, "my-id");
        }

        [Test]
        public void Envelope_attributes_should_be_present()
        {
            var offset = DateTimeOffset.UtcNow;
            var builder = new MessageEnvelopeBuilder("my-id");
            builder.Attributes["Custom"] = 1;
            builder.Attributes[EnvelopeAttributes.CreatedUtc] = offset;

            var envelope = RoundtripViaSerializer(builder);

            Assert.AreEqual(1, envelope.GetAttribute("Custom"));
            Assert.AreEqual(offset, envelope.GetAttribute(EnvelopeAttributes.CreatedUtc));
        }

        [Test]
        public void Payload_should_be_serialized()
        {
            var builder = new MessageEnvelopeBuilder("my-id");
            builder.AddItem(new MyMessage("42"));

            var envelope = RoundtripViaSerializer(builder);

            Assert.AreEqual(1, envelope.Items.Length);
            Assert.AreEqual("42", ((MyMessage)envelope.Items[0].Content).Value);
        }

        [Test]
        public void Multiple_payloads_are_handled_in_sequence()
        {
            var builder = new MessageEnvelopeBuilder("my-id");

            for (int i = 0; i < 5; i++)
            {
                var content = new string('*',i);
                builder.AddItem(new MyMessage(content));
            }

            var envelope = RoundtripViaSerializer(builder);

            Assert.AreEqual(5, envelope.Items.Length);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(new string('*', i), ((MyMessage)envelope.Items[i].Content).Value);
            }
        }

      

        [Serializable]
        sealed class MyMessage 
        {
            
            public readonly string Value;

            public MyMessage(string value)
            {
                Value = value;
            }
            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}