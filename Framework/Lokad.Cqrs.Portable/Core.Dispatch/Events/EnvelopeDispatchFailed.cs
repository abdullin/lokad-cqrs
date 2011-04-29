#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class EnvelopeDispatchFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public ImmutableMessageEnvelope Envelope { get; private set; }
        public string QueueName { get; private set; }

        public EnvelopeDispatchFailed(ImmutableMessageEnvelope envelope, string queueName, Exception exception)
        {
            Exception = exception;
            Envelope = envelope;
            QueueName = queueName;
        }

        public override string ToString()
        {
            return string.Format("Failed to consume {0} from '{1}': {2}", Envelope.EnvelopeId, QueueName, Exception.Message);
        }
    }
}