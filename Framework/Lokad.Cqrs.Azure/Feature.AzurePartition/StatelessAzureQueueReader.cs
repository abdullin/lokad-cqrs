#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.AzurePartition.Events;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class StatelessAzureQueueReader
    {
        readonly IEnvelopeStreamer _streamer;
        readonly TimeSpan _visibilityTimeout;
        readonly ISystemObserver _observer;

        readonly CloudBlobContainer _cloudBlob;
        readonly Lazy<CloudQueue> _posionQueue;
        readonly CloudQueue _queue;
        readonly string _queueName;

        public string Name
        {
            get { return _queueName; }
        }


        public StatelessAzureQueueReader(
            string name,
            CloudQueue primaryQueue,
            CloudBlobContainer container,
            Lazy<CloudQueue> poisonQueue,
            ISystemObserver provider,
            IEnvelopeStreamer streamer, TimeSpan visibilityTimeout)
        {
            _cloudBlob = container;
            _queue = primaryQueue;
            _posionQueue = poisonQueue;
            _observer = provider;
            _queueName = name;
            _streamer = streamer;
            _visibilityTimeout = visibilityTimeout;
        }

        public void Initialize()
        {
            _queue.CreateIfNotExist();
            _cloudBlob.CreateIfNotExist();
            
        }

        public GetEnvelopeResult TryGetMessage()
        {
            CloudQueueMessage message;
            try
            {
                message = _queue.GetMessage(_visibilityTimeout);
            }
            catch (Exception ex)
            {
                _observer.Notify(new FailedToReadMessage(ex, _queueName));
                return GetEnvelopeResult.Error();
            }

            if (null == message)
            {
                return GetEnvelopeResult.Empty;
            }
            
            try
            {
                var unpacked = DownloadPackage(message);
                return GetEnvelopeResult.Success(unpacked);
            }
            catch (StorageClientException ex)
            {
                _observer.Notify(new FailedToAccessStorage(ex, _queue.Name, message.Id));
                return GetEnvelopeResult.Retry;
            }
            catch (Exception ex)
            {
                _observer.Notify(new EnvelopeDeserializationFailed(ex, _queue.Name, message.Id));


                // new poison details
                _posionQueue.Value.AddMessage(message);
                _queue.DeleteMessage(message);
                return GetEnvelopeResult.Retry;
            }
        }

        EnvelopeTransportContext DownloadPackage(CloudQueueMessage message)
        {
            var buffer = message.AsBytes;

            EnvelopeReference reference;
            if (_streamer.TryReadAsEnvelopeReference(buffer, out reference))
            {
                if (reference.StorageContainer != _cloudBlob.Uri.ToString())
                    throw new InvalidOperationException("Wrong container used!");
                var blob = _cloudBlob.GetBlobReference(reference.StorageReference);
                buffer = blob.DownloadByteArray();
            }
            var m = _streamer.ReadAsEnvelopeData(buffer);
            return new EnvelopeTransportContext(message, m, _queueName);
        }


        /// <summary>
        /// ACKs the message by deleting it from the queue.
        /// </summary>
        /// <param name="envelope">The message context to ACK.</param>
        public void AckMessage(EnvelopeTransportContext envelope)
        {
            if (envelope == null) throw new ArgumentNullException("message");
            _queue.DeleteMessage((CloudQueueMessage) envelope.TransportMessage);
        }

        public void TryNotifyNack(EnvelopeTransportContext context)
        {
            // we don't do anything. Azure queues have visibility
        }
    }
}