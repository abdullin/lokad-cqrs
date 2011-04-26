#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition.Events
{
    public sealed class FailedToAccessStorage : ISystemEvent
    {
        public StorageClientException Exception { get; private set; }
        public string QueueName { get; private set; }
        public string MessageId { get; private set; }

        public FailedToAccessStorage(StorageClientException exception, string queueName, string messageId)
        {
            Exception = exception;
            QueueName = queueName;
            MessageId = messageId;
        }
    }
}