#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.AzureConsumer.Events;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class StatelessAzureQueueReader 
	{
		readonly IMessageSerializer _serializer;
		readonly ISystemObserver _observer;

		readonly CloudBlobContainer _cloudBlob;
		readonly Lazy<CloudQueue> _posionQueue;
		readonly CloudQueue _queue;
		readonly string _queueName;

		public string Name { get { return _queueName; } }

		const int RetryCount = 4;

		public StatelessAzureQueueReader(
			CloudStorageAccount account,
			string queueName,
			ISystemObserver provider,
			IMessageSerializer serializer)
		{
			var blobClient = account.CreateCloudBlobClient();
			blobClient.RetryPolicy = RetryPolicies.NoRetry();

			_cloudBlob = blobClient.GetContainerReference(queueName);

			var queueClient = account.CreateCloudQueueClient();
			queueClient.RetryPolicy = RetryPolicies.NoRetry();
			_queue = queueClient.GetQueueReference(queueName);
			_posionQueue = new Lazy<CloudQueue>(() =>
				{
					var queue = queueClient.GetQueueReference(queueName + "-poison");
					queue.CreateIfNotExist();
					return queue;
				}, LazyThreadSafetyMode.ExecutionAndPublication); 

			_observer = provider;

			_queueName = queueName;
			_serializer = serializer;
		}

		

		public void Init()
		{
			_queue.CreateIfNotExist();
			_cloudBlob.CreateIfNotExist();
		}

		public GetMessageResult TryGetMessage()
		{
			CloudQueueMessage message;
			try
			{
				message = _queue.GetMessage();
			}
			catch (Exception ex)
			{
				_observer.Notify(new FailedToReadMessage(ex, _queueName));
				return GetMessageResult.Error();
			}

			if (null == message)
			{
				return GetMessageResult.Empty;
			}

			if (message.DequeueCount > RetryCount)
			{
				_observer.Notify(new RetrievedPoisonMessage(_queue.Name, message.Id));
				
				_posionQueue.Value.AddMessage(message);
				_queue.DeleteMessage(message);
				return GetMessageResult.Retry;
			}

			try
			{
				var m = MessageUtil.ReadMessage(message.AsBytes, _serializer, DownloadPackage);
				var unpacked = new MessageContext(message, m, _queueName);
				return GetMessageResult.Success(unpacked);
			}
			catch (StorageClientException ex)
			{
				_observer.Notify(new FailedToAccessStorage(ex, _queue.Name, message.Id));
				return GetMessageResult.Retry;
			}
			catch (Exception ex)
			{
				_observer.Notify(new FailedToDeserializeMessage(ex, _queue.Name, message.Id));
				
				// new poison details
				_posionQueue.Value.AddMessage(message);
				_queue.DeleteMessage(message);
				return GetMessageResult.Retry;
			}
		}

		byte[] DownloadPackage(MessageReference reference)
		{
			if (reference.StorageContainer != _cloudBlob.Uri.ToString())
				throw new InvalidOperationException("Wrong container used!");
			var blob = _cloudBlob.GetBlobReference(reference.StorageReference);
			return blob.DownloadByteArray();
		}

		/// <summary>
		/// ACKs the message by deleting it from the queue.
		/// </summary>
		/// <param name="message">The message context to ACK.</param>
		public void AckMessage(MessageContext message)
		{
			if (message == null) throw new ArgumentNullException("message");
			_queue.DeleteMessage((CloudQueueMessage)message.TransportMessage);
			_observer.Notify( new MessageAcked(_queueName, message.Unpacked.EnvelopeId));
		}

		public void TryNotifyNack(MessageContext context)
		{
			// we don't do anything. Azure queues have visibility
		}
	}
}