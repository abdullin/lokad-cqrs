#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Lmf;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
namespace Lokad.Cqrs.Queue
{
	public sealed class AzureReadQueue
	{
		readonly string _queueName;
		readonly IMessageSerializer _serializer;

		readonly CloudBlobContainer _cloudBlob;
		readonly ILog _log;
		
		readonly CloudQueue _posionQueue;
		readonly CloudQueue _queue;
		const int RetryCount = 4;

		public AzureReadQueue(
			CloudStorageAccount account,
			string queueName,
			ILogProvider provider,
			IMessageSerializer serializer)
		{
			var blobClient = account.CreateCloudBlobClient();
			blobClient.RetryPolicy = RetryPolicies.NoRetry();

			_cloudBlob = blobClient.GetContainerReference(queueName);

			var queueClient = account.CreateCloudQueueClient();
			queueClient.RetryPolicy = RetryPolicies.NoRetry();
			_queue = queueClient.GetQueueReference(queueName);
			_posionQueue = queueClient.GetQueueReference(queueName + "-poison");

			_log = provider.Get("Queue[" + queueName + "]");

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
			_posionQueue.CreateIfNotExist();
			_cloudBlob.CreateIfNotExist();
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
				return GetMessageResult.Error(ex);
			}

			if (null == message)
			{
				return GetMessageResult.Wait;
			}

			if (message.DequeueCount > RetryCount)
			{
				// we consider this to be poison
				_log.ErrorFormat("Moving message {0} to poison queue {1}", message.Id, _posionQueue.Name);
				// Move to poison
				_posionQueue.AddMessage(message);
				_queue.DeleteMessage(message);
				return GetMessageResult.Retry;
			}

			try
			{
				var unpacked = GetMessageFromCloud(message, message.AsBytes);
				return GetMessageResult.Success(unpacked);
			}
			catch (StorageClientException ex)
			{
				_log.WarnFormat(ex, "Storage access problems for {0}", message.Id);
				return GetMessageResult.Retry;
			}
			catch (Exception ex)
			{
				_log.ErrorFormat(ex, "Failed to deserialize envelope {0}. Moving to poison", message.Id);
				// new poison details
				_posionQueue.AddMessage(message);
				_queue.DeleteMessage(message);
				return GetMessageResult.Retry;
			}
		}

		UnpackedMessage GetMessageFromCloud(CloudQueueMessage cloud, byte[] buffer)
		{
			// unefficient reading for now, since protobuf-net does not support reading parts
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion == MessageHeader.DataMessageFormatVersion)
			{
				return MessageUtil.ReadDataMessage(buffer, _serializer).WithState(cloud);
			}
			if (header.MessageFormatVersion == MessageHeader.ReferenceMessageFormatVersion)
			{
				var reference = MessageUtil.ReadReferenceMessage(buffer);

				var blob = _cloudBlob.GetBlobReference(reference);
				var currentBuffer = blob.DownloadByteArray();
				return GetMessageFromCloud(cloud, currentBuffer);
			}
			throw Errors.InvalidOperation("Unknown message format: {0}", header.MessageFormatVersion);
		}

		public void AckMessage(UnpackedMessage message)
		{
			if (message == null) throw new ArgumentNullException("message");

			var cloud = message.GetState<CloudQueueMessage>().Value;
			_log.Debug(message);
			_queue.DeleteMessage(cloud);
		}
	}
}