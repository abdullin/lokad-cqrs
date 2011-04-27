using System;

namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class QuarantinedMessage : ISystemEvent
    {
        public Exception LastException { get; private set; }
        public string EnvelopeId { get; private set; }
        public string QueueName { get; private set; }

        public QuarantinedMessage(Exception lastException, string envelopeId, string queueName)
        {
            LastException = lastException;
            EnvelopeId = envelopeId;
            QueueName = queueName;
        }
    }

    public sealed class EventHadNoConsumers : ISystemEvent
    {
        public string EnvelopeId { get; private set; }
        public Type EventItemType { get; private set; }

        public EventHadNoConsumers(string envelopeId, Type eventItemType)
        {
            EnvelopeId = envelopeId;
            EventItemType = eventItemType;
        }

        public override string ToString()
        {
            return string.Format("Event '{0}' had no consumers {1}", EnvelopeId, EventItemType);
        }
    }
}