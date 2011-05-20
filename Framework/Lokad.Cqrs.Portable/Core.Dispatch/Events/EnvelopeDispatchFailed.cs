#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;

namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class EnvelopeDispatchFailed : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public ImmutableEnvelope Envelope { get; private set; }
        public string QueueName { get; private set; }

        public EnvelopeDispatchFailed(ImmutableEnvelope envelope, string queueName, Exception exception)
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

    public sealed class EnvelopeDispatchStarted : ISystemEvent
    {
        public string QueueName { get; private set; }
        public Type[] MappedTypes { get; private set; }
        public string EnvelopeId { get; private set; }

        public EnvelopeDispatchStarted(string queueName, Type[] mappedTypes, string envelopeId)
        {
            QueueName = queueName;
            MappedTypes = mappedTypes;
            EnvelopeId = envelopeId;
        }

        public override string ToString()
        {
            return string.Format("Starting dispatch of ({0}) from '{1}' [{2}] ",
                string.Join(",", MappedTypes.Select(t => t.Name).ToArray()), QueueName, EnvelopeId);
        }
    }

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