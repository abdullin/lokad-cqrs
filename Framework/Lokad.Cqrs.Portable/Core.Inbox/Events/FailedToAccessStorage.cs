#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Core.Inbox.Events
{
    public sealed class FailedToAccessStorage : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public string QueueName { get; private set; }
        public string MessageId { get; private set; }

        public FailedToAccessStorage(Exception exception, string queueName, string messageId)
        {
            Exception = exception;
            QueueName = queueName;
            MessageId = messageId;
        }
        public override string ToString()
        {
            return string.Format("Failed to read '{0}' from '{1}': {2}", MessageId, QueueName, Exception.Message);
        }
    }
}