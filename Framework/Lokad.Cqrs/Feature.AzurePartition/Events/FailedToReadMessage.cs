#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AzurePartition.Events
{
    public sealed class FailedToReadMessage : ISystemEvent
    {
        public Exception Exception { get; private set; }
        public string QueueName { get; private set; }

        public FailedToReadMessage(Exception exception, string queueName)
        {
            Exception = exception;
            QueueName = queueName;
        }

        public override string ToString()
        {
            return string.Format("Failed to read from '{0}': {1}", QueueName, Exception.Message);
        }
    }
}