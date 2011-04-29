using System;

namespace Lokad.Cqrs.Core.Dispatch.Events
{
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