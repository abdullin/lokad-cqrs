#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs.Lmf;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
namespace Lokad.Cqrs.Queue
{
	public sealed class AzureMessageQueue
	{
		public const string DateFormatInBlobName = "yyyy-MM-dd-HH-mm-ss-ffff";
		readonly CloudBlobContainer _cloudBlob;
		readonly ILog _log;
		readonly IMessageSerializer _serializer;
		readonly CloudQueue _posionQueue;
		readonly CloudQueue _queue;
		readonly AzureQueueReference _queueReference;
		readonly int _retryCount;

		public AzureMessageQueue(
			CloudStorageAccount account,
			string queueName,
			int retryCount,
			ILogProvider provider,
			IMessageSerializer serializer)
		{
			var blobClient = account.CreateCloudBlobClient();
			blobClient.RetryPolicy = RetryPolicies.NoRetry();

			_cloudBlob = blobClient.GetContainerReference(queueName);

			var queueClient = account.CreateCloudQueueClient();
			queueClient.RetryPolicy = RetryPolicies.NoRetry();
			_queue = queueClient.GetQueueReference(queueName);

			_queueReference = new AzureQueueReference(account.QueueEndpoint, _queue.Name);
			_posionQueue = queueClient.GetQueueReference(_queueReference.SubQueue("poison").QueueName);

			_log = provider.Get("Queue[" + queueName + "]");

			_serializer = serializer;
			_retryCount = retryCount;
		}

		public TimeSpan? QueueVisibility { get; set; }

		public Uri Uri
		{
			get { return _queueReference.Uri; }
		}

		public void Init()
		{
			if (_queue.CreateIfNotExist())
				_log.DebugFormat("Auto-created queue {0}", _queue.Uri);

			if (_posionQueue.CreateIfNotExist())
				_log.DebugFormat("Auto-created poison queue {0}", _posionQueue.Uri);

			if (_cloudBlob.CreateIfNotExist())
				_log.DebugFormat("Auto-created blob storage {0}", _cloudBlob.Uri);
		}

		public GetMessageResult GetMessage()
		{
			CloudQueueMessage message;
			try
			{
				message = QueueVisibility.HasValue
					? _queue.GetMessage(QueueVisibility.Value)
					: _queue.GetMessage();
			}
			catch (Exception ex)
			{
				return GetMessageResult.Error(ex);
			}

			if (null == message)
			{
				return GetMessageResult.Wait;
			}

			if (message.DequeueCount > _retryCount)
			{
				// we consider this to be poison
				_log.ErrorFormat("Moving message {0} to poison queue {1}", message.Id, _posionQueue.Name);
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
				MoveIncomingToPoison(message);
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

		public void SendMessages(object[] messages, Action<MessageAttributeBuilder> headers)
		{
			if (messages.Length == 0)
				return;
			foreach (var message in messages)
			{
				var packed = PackNewMessage(message, headers);
				_queue.AddMessage(packed);
			}
		}

		void MoveIncomingToPoison(CloudQueueMessage message)
		{
			// new poison details
			_posionQueue.AddMessage(message);
			_queue.DeleteMessage(message);
		}

		//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
		const int CloudQueueLimit = 6144;


		CloudQueueMessage PackNewMessage(object message,  Action<MessageAttributeBuilder> modify)
		{
			var messageId = Guid.NewGuid();
			var created = DateTime.UtcNow;

			var messageType = message.GetType();
			var contract = _serializer
				.GetContractNameByType(messageType)
				.ExposeException(() => QueueErrors.NoContractNameOnSend(messageType, _serializer));

			var referenceId = created.ToString(DateFormatInBlobName) + "-" + messageId;

			var builder = new MessageAttributeBuilder();
			builder.AddTopic(contract);
			builder.AddContract(contract);
			builder.AddSender(_queue.Uri.ToString());
			builder.AddIdentity(messageId.ToString());
			builder.AddCreated(created);
			modify(builder);
			var attributes = builder.Build();

			using (var stream = MessageUtil.SaveDataMessageToStream(attributes, s => _serializer.Serialize(message, s)))
			{
				if (stream.Length < CloudQueueLimit)
				{
					// write message to queue
					return new CloudQueueMessage(stream.ToArray());
				}
				// write message to blob
				stream.Seek(0, SeekOrigin.Begin);
				_cloudBlob
					.GetBlobReference(referenceId)
					.UploadFromStream(stream);
			}

			// ok, we didn't fit, so create reference message
			var reference = new MessageAttributeBuilder();
			reference.AddBlobReference(_cloudBlob.Uri, referenceId);
			reference.AddTopic(contract);
			reference.AddContract(contract);

			// write reference message
			using (var stream = MessageUtil.SaveReferenceMessageToStream(reference.Build()))
			{
				return new CloudQueueMessage(stream.ToArray());
			}
		}
	}
}