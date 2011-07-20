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

        public override string ToString()
        {
            return string.Format("Quarantined '{0}' of '{1}'", Envelope.EnvelopeId, QueueName);
        }
    }
}