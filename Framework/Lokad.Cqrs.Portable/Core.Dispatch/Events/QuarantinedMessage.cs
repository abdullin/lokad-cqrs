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
}