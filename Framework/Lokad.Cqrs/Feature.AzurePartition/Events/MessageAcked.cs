#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AzurePartition.Events
{
    public sealed class MessageAcked : ISystemEvent
    {
        public string QueueName { get; private set; }
        public string EnvelopeId { get; private set; }

        public MessageAcked(string queueName, string envelopeId)
        {
            QueueName = queueName;
            EnvelopeId = envelopeId;
        }

        public override string ToString()
        {
            return string.Format("Message {0} acked at {1}", EnvelopeId, QueueName);
        }
    }
}