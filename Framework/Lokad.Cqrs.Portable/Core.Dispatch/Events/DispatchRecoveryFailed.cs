using System;

namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class DispatchRecoveryFailed : ISystemEvent
    {
        public Exception DispatchException { get; private set; }
        public ImmutableEnvelope Envelope { get; private set; }
        public string QueueName { get; private set; }

        public DispatchRecoveryFailed(Exception exception, ImmutableEnvelope envelope, string queueName)
        {
            DispatchException = exception;
            Envelope = envelope;
            QueueName = queueName;
        }
    }
}