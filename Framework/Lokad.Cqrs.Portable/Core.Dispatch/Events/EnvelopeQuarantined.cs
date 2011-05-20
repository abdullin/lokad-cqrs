using System;

namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class EnvelopeQuarantined : ISystemEvent
    {
        public Exception LastException { get; private set; }
        public ImmutableEnvelope Envelope { get; private set; }
        public string QueueName { get; private set; }

        public EnvelopeQuarantined(Exception lastException, ImmutableEnvelope envelope, string queueName)
        {
            LastException = lastException;
            Envelope = envelope;
            QueueName = queueName;
        }
    }

    public sealed class EnvelopeDuplicateDiscarded : ISystemEvent
    {
        public string QueueName { get; private set; }
        public string EnvelopeId { get; private set; }

        public EnvelopeDuplicateDiscarded(string queueName, string envelopeId)
        {
            QueueName = queueName;
            EnvelopeId = envelopeId;
        }
    }
}