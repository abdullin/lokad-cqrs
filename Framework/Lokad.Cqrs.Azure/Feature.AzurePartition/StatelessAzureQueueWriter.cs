#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Outbox;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class StatelessAzureQueueWriter : IQueueWriter
    {
        public void PutMessage(ImmutableEnvelope envelope)
        {
            var packed = PrepareCloudMessage(envelope);
            _queue.AddMessage(packed);
        }

        //http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
        const int CloudQueueLimit = 6144;


        CloudQueueMessage PrepareCloudMessage(ImmutableEnvelope builder)
        {
            var buffer = _streamer.SaveDataMessage(builder);
            if (buffer.Length < CloudQueueLimit)
            {
                // write message to queue
                return new CloudQueueMessage(buffer);
            }
            // ok, we didn't fit, so create reference message
            var referenceId = DateTimeOffset.UtcNow.ToString(DateFormatInBlobName) + "-" + builder.EnvelopeId;
            _cloudBlob.GetBlobReference(referenceId).UploadByteArray(buffer);
            var reference = new EnvelopeReference(builder.EnvelopeId, _cloudBlob.Uri.ToString(), referenceId);
            var blob = _streamer.SaveReferenceMessage(reference);
            return new CloudQueueMessage(blob);
        }

        public StatelessAzureQueueWriter(IEnvelopeStreamer streamer, CloudBlobContainer container, CloudQueue queue)
        {
            _streamer = streamer;
            _cloudBlob = container;
            _queue = queue;
        }

        public void Init()
        {
            _queue.CreateIfNotExist();
            _cloudBlob.CreateIfNotExist();
        }


        const string DateFormatInBlobName = "yyyy-MM-dd-HH-mm-ss-ffff";
        readonly IEnvelopeStreamer _streamer;
        readonly CloudBlobContainer _cloudBlob;
        readonly CloudQueue _queue;
    }
}