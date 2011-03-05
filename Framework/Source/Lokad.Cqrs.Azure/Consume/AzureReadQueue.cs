#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using Lokad.Cqrs.Durability;
using Lokad.Cqrs.Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Consume
{
	public sealed class AzureReadQueue
	{
		readonly IMessageSerializer _serializer;
		readonly ISystemObserver _observer;

		readonly CloudBlobContainer _cloudBlob;
		readonly Lazy<CloudQueue> _posionQueue;
		readonly CloudQueue _queue;
		readonly string _queueName;

		const int RetryCount = 4;

		public AzureReadQueue(
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

		
		public string Name
		{
			get { return _queueName; }
		}

		public void Init()
		{
			_queue.CreateIfNotExist();
			_cloudBlob.CreateIfNotExist();
			
			// this one will be initilized on-demand
			//_posionQueue.CreateIfNotExist();
		}

		public GetMessageResult GetMessage()
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
				return GetMessageResult.Wait;
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
				var unpacked = new AzureMessageContext(message, m);
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

		public void AckMessage(AzureMessageContext message)
		{
			if (message == null) throw new ArgumentNullException("message");
			_queue.DeleteMessage(message.CloudMessage);
			_observer.Notify( new MessageAcked(_queueName, message.Unpacked.EnvelopeId));
		}
	}

	public sealed class MessageAcked : ISystemEvent
	{
		public string QueueName { get; private set; }
		public string EnvelopeId { get; private set; }

		public MessageAcked(string queueName, string envelopeId)
		{
			QueueName = queueName;
			EnvelopeId = envelopeId;
		}
	}

	public sealed class RetrievedPoisonMessage : ISystemEvent

	{
		public string QueueName { get; private set; }
		public string MessageId { get; private set; }

		public RetrievedPoisonMessage(string queueName, string messageId)
		{
			QueueName = queueName;
			MessageId = messageId;
		}
	}

	public sealed class FailedToReadMessage : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string QueueName { get; private set; }

		public FailedToReadMessage(Exception exception, string queueName)
		{
			Exception = exception;
			QueueName = queueName;
		}
	}


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

	public sealed class FailedToDeserializeMessage : ISystemEvent
	{
		public Exception Exception { get; private set; }
		public string QueueName { get; private set; }
		public string MessageId { get; private set; }

		public FailedToDeserializeMessage(Exception exception, string queueName, string messageId)
		{
			Exception = exception;
			QueueName = queueName;
			MessageId = messageId;
		}
	}

	public sealed class AzureMessageContext
	{
		public readonly CloudQueueMessage CloudMessage;
		public readonly MessageEnvelope Unpacked;

		public AzureMessageContext(CloudQueueMessage cloudMessage, MessageEnvelope unpacked)
		{
			CloudMessage = cloudMessage;
			Unpacked = unpacked;
		}
	}


}