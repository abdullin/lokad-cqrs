#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Transactions;
using Lokad.Cqrs.Serialization;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using ProtoBuf;

namespace Lokad.Cqrs.Queue
{
	public sealed class AzureMessageQueue : IReadMessageQueue, IWriteMessageQueue
	{
		public const string DateFormatInBlobName = "yyyy-MM-dd-HH-mm-ss-ffff";
		readonly CloudStorageAccount _account;
		readonly CloudBlobContainer _cloudBlob;
		readonly IMessageProfiler _debugger;
		readonly CloudQueue _discardQueue;
		readonly ILog _log;
		readonly IMessageSerializer _messageSerializer;
		readonly CloudQueue _posionQueue;
		readonly CloudQueue _queue;


		readonly AzureQueueReference _queueReference;
		readonly int _retryCount;


		public AzureMessageQueue(
			CloudStorageAccount account,
			string queueName,
			int retryCount,
			ILogProvider provider,
			IMessageSerializer messageSerializer, IMessageProfiler debugger)
		{
			var blobClient = account.CreateCloudBlobClient();
			blobClient.RetryPolicy = RetryPolicies.NoRetry();

			_cloudBlob = blobClient.GetContainerReference(queueName);

			var queueClient = account.CreateCloudQueueClient();
			queueClient.RetryPolicy = RetryPolicies.NoRetry();
			_queue = queueClient.GetQueueReference(queueName);

			_queueReference = new AzureQueueReference(account.QueueEndpoint, _queue.Name);
			_posionQueue = queueClient.GetQueueReference(_queueReference.SubQueue(SubQueueType.Poison).QueueName);
			_discardQueue = queueClient.GetQueueReference(_queueReference.SubQueue(SubQueueType.Discard).QueueName);

			_log = provider.Get("Queue[" + queueName + "]");


			_account = account;
			_debugger = debugger;

			_messageSerializer = messageSerializer;
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

			if (_discardQueue.CreateIfNotExist())
				_log.DebugFormat("Auto-created discard queue {0}", _discardQueue.Uri);

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
				TransactionalMoveMessage(message, _posionQueue);
				return GetMessageResult.Retry;
			}

			try
			{
				var buffer = message.AsBytes;
				var unpacked = GetMessageFromBuffer(buffer, message);
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

		static MessageAttribute[] GetAttributesFromBuffer(byte[] queueAsBytes, MessageHeader header)
		{
			MessageAttribute[] attributes;
			using (var stream = new MemoryStream(queueAsBytes, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				var contract = Serializer.Deserialize<MessageAttributes>(stream);
				attributes = contract.Attributes;
			}
			return attributes;
		}

		static MessageHeader GetHeaderFromBuffer(byte[] queueAsBytes)
		{
			using (var stream = new MemoryStream(queueAsBytes, 0, MessageHeader.FixedSize))
			{
				return Serializer.Deserialize<MessageHeader>(stream);
			}
		}

		UnpackedMessage GetMessageFromBuffer(byte[] buffer, CloudQueueMessage cloud)
		{
			// unefficient reading for now, since protobuf-net does not support reading parts
			var header = GetHeaderFromBuffer(buffer);
			if (header.MessageFormatVersion == MessageHeader.CommonMessageFormatVersion)
			{
				var attributes = GetAttributesFromBuffer(buffer, header);
				return UnpackMessage(buffer, header, attributes, cloud);
			}
			if (header.MessageFormatVersion == MessageHeader.ReferenceMessageFormatVersion)
			{
				var attributes = GetAttributesFromBuffer(buffer, header);
				var reference = attributes
					.GetLastString(MessageAttributeType.StorageReference)
					.ExposeException("Protocol violation: message should have storage reference", MessageAttributeType.StorageReference);
				var blob = _cloudBlob.GetBlobReference(reference);
				buffer = blob.DownloadByteArray();
				return GetMessageFromBuffer(buffer, cloud);
			}
			throw Errors.InvalidOperation("Unknown message format: {0}", header.MessageFormatVersion);
		}

		UnpackedMessage UnpackMessage(byte[] queueAsBytes, MessageHeader header, MessageAttribute[] attributes,
			CloudQueueMessage cloud)
		{
			UnpackedMessage unpacked;
			string contract = attributes
				.GetLastString(MessageAttributeType.ContractName)
				.ExposeException("Protocol violation: message should have contract name");
			var type = _messageSerializer.GetTypeByContractName(contract);

			using (
				var stream = new MemoryStream(queueAsBytes, MessageHeader.FixedSize + (int) header.AttributesLength,
					(int) header.ContentLength))
			{
				var instance = _messageSerializer.Deserialize(stream, type);
				unpacked = new UnpackedCloudMessage(header, attributes, instance, cloud, type);
			}
			return unpacked;
		}

		public void AckMessage(UnpackedMessage message)
		{
			Enforce.Argument(() => message);
			var cloudMessage = message as UnpackedCloudMessage;

			if (null == cloudMessage)
			{
				throw Errors.InvalidOperation("Can't ACK message '{0}'", message);
			}

			if (_log.IsDebugEnabled())
			{
				_log.Debug(_debugger.GetReadableMessageInfo(message));
			}
			var id = cloudMessage.TransportMessageId;
			var receipt = cloudMessage.PopReceipt;
			TransactionalDeleteMessage(id, receipt);
		}

		public void DiscardMessage(UnpackedMessage unpacked)
		{
			Enforce.Argument(() => unpacked);

			var message = unpacked as UnpackedCloudMessage;
			if (message == null)
			{
				throw Errors.InvalidOperation("Can't discard message '{0}'", unpacked);
			}
			// new discarded message
			// could be optimized by copying blob reference
			var packed = PackNewMessage(message.Content, nv => { });


			if (Transaction.Current == null)
			{
				_discardQueue.AddMessage(packed);
				_queue.DeleteMessage(packed);
			}
			else
			{
				Transaction.Current.EnlistVolatile(
					new TransactionCommitDeletesMessage(_queue, message.TransportMessageId, message.PopReceipt),
					EnlistmentOptions.None);
				Transaction.Current.EnlistVolatile(
					new TransactionCommitAddsMessage(_discardQueue, packed),
					EnlistmentOptions.None);
			}
		}

		public void SendMessages(object[] messages, Action<MessageAttributeBuilder> headers)
		{
			if (messages.Length == 0)
				return;
			foreach (var message in messages)
			{
				var packed = PackNewMessage(message, headers);
				TransactionalSendMessage(_queue, packed);
			}
		}

		public void RouteMessages(UnpackedMessage[] messages, Action<MessageAttributeBuilder> headers)
		{
			foreach (var message in messages)
			{
				var packed = PackNewMessage(message.Content, headers);
				TransactionalSendMessage(_queue, packed);
			}
		}

		public void Purge()
		{
			_queue.Clear();
			_posionQueue.Clear();
		}

		void TransactionalMoveMessage(CloudQueueMessage message, CloudQueue target)
		{
			if (Transaction.Current == null)
			{
				target.AddMessage(message);
				_queue.DeleteMessage(message);
			}
			else
			{
				Transaction.Current.EnlistVolatile(
					new TransactionCommitDeletesMessage(_queue, message.Id, message.PopReceipt),
					EnlistmentOptions.None);
				Transaction.Current.EnlistVolatile(
					new TransactionCommitAddsMessage(target, message),
					EnlistmentOptions.None);
			}
		}

		void MoveIncomingToPoison(CloudQueueMessage message)
		{
			// new poison details
			TransactionalMoveMessage(message, _posionQueue);
		}

		void TransactionalDeleteMessage(string id, string receipt)
		{
			if (Transaction.Current == null)
			{
				_queue.DeleteMessage(id, receipt);
			}
			else
			{
				Transaction.Current.EnlistVolatile(new TransactionCommitDeletesMessage(_queue, id, receipt), EnlistmentOptions.None);
			}
		}

		//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
		const int CloudQueueLimit = 6144;


		CloudQueueMessage PackNewMessage(object message, Action<MessageAttributeBuilder> modify)
		{
			var messageId = GuidUtil.NewComb();
			var created = SystemUtil.UtcNow;

			var contract = _messageSerializer.GetContractNameByType(message.GetType());

			var referenceId = created.ToString(DateFormatInBlobName) + "-" + messageId;

			var builder = new MessageAttributeBuilder();
			builder.AddTopic(contract);
			builder.AddSender(_queue.Uri.ToString());


			builder.AddContract(contract);
			builder.AddIdentity(messageId.ToString());

			builder.AddCreated(created);
			modify(builder);
			var messageAttributes = builder.Build();

			using (var stream = new MemoryStream())
			{
				// skip header
				stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

				// save attributes

				Serializer.Serialize(stream, messageAttributes);
				var attributesLength = stream.Position - MessageHeader.FixedSize;

				// save message
				_messageSerializer.Serialize(message, stream);
				var bodyLength = stream.Position - attributesLength - MessageHeader.FixedSize;

				var totalLength = stream.Position;
				// write the header
				stream.Seek(0, SeekOrigin.Begin);
				Serializer.Serialize(stream, MessageHeader.ForData(attributesLength, bodyLength, 0));

				// write message to queue
				if (totalLength < CloudQueueLimit)
				{
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
			using (var stream = new MemoryStream())
			{
				// skip header
				stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

				// write reference
				Serializer.Serialize(stream, reference.Build());
				long attributesLength = stream.Position - MessageHeader.FixedSize;
				// write header
				stream.Seek(0, SeekOrigin.Begin);
				Serializer.Serialize(stream, MessageHeader.ForReference(attributesLength, 0));
				// create queue
				return new CloudQueueMessage(stream.ToArray());
			}
		}


		void TransactionalSendMessage(CloudQueue queue, CloudQueueMessage cloudQueueMessage)
		{
			if (Transaction.Current == null)
			{
				queue.AddMessage(cloudQueueMessage);
			}
			else
			{
				Transaction.Current.EnlistVolatile(
					new TransactionCommitAddsMessage(queue, cloudQueueMessage),
					EnlistmentOptions.None);
			}
		}
	}
}