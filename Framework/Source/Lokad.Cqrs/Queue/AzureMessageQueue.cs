#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using System.IO;
using System.Transactions;
using Lokad.Cqrs.Serialization;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

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

			IncomingMessageEnvelope envelope;

			try
			{
				using (var stream = new MemoryStream(message.AsBytes))
				{
					var data = (MessageMessage) _messageSerializer.Deserialize(stream, typeof (MessageMessage));
					envelope = new IncomingMessageEnvelope(data, message);
				}
			}
			catch (Exception ex)
			{
				_log.ErrorFormat(ex, "Failed to deserialize message {0}. Moving to poison", message.Id);
				TransactionalMoveMessage(message, _posionQueue);
				return GetMessageResult.Retry;
			}

			if (envelope.ContainsMessageObject)
			{
				var m = new IncomingMessage(envelope.MessageObject, envelope);


				return GetMessageResult.Success(m);
			}

			_log.DebugFormat("Message {0} references data {1}; downloading it.", message.Id, envelope.DataReference);

			Type messageType;

			try
			{
				messageType = _messageSerializer.GetTypeByContractName(envelope.TypeReference);
			}
			catch (Exception ex)
			{
				_log.ErrorFormat(ex, "Failed to discover type '{0}' of message {1}",
					envelope.TypeReference,
					message.Id);
				MoveIncomingToPoison(message);
				return GetMessageResult.Retry;
			}

			var blobReference = _cloudBlob.GetBlobReference(envelope.DataReference);
			var bytes = blobReference.DownloadByteArray();

			try
			{
				using (var stream = new MemoryStream(bytes))
				{
					var data = _messageSerializer.Deserialize(stream, messageType);
					var m = new IncomingMessage(data, envelope);
					return GetMessageResult.Success(m);
				}
			}
			catch (Exception ex)
			{
				_log.ErrorFormat(ex, "Failed to deserialize message {0}. Moving to poison", message.Id);
				MoveIncomingToPoison(message);
				return GetMessageResult.Retry;
			}
		}

		public void AckMessage(IncomingMessage message)
		{
			var id = message.TransportMessageId;
			var receipt = message.Headers["azure-receipt"];

			if (_log.IsDebugEnabled())
			{
				_log.Debug(_debugger.GetReadableMessageInfo(message.Message, message.TransportMessageId));
			}
			TransactionalDeleteMessage(id, receipt);
		}

		public void DiscardMessage(IncomingMessage message)
		{
			var receipt = message.Headers["azure-receipt"];
			if (string.IsNullOrEmpty(receipt))
			{
				throw new InvalidOperationException("Can't discard non-azure message");
			}
			// new discarded message
			// could be optimized by copying blob reference
			var packed = PackToNewMessage(nv => { }, message.Message);


			if (Transaction.Current == null)
			{
				_discardQueue.AddMessage(packed);
				_queue.DeleteMessage(packed);
			}
			else
			{
				Transaction.Current.EnlistVolatile(
					new TransactionCommitDeletesMessage(_queue, message.TransportMessageId, receipt),
					EnlistmentOptions.None);
				Transaction.Current.EnlistVolatile(
					new TransactionCommitAddsMessage(_discardQueue, packed),
					EnlistmentOptions.None);
			}
		}

		public void SendMessages(object[] messages, Action<NameValueCollection> headers)
		{
			if (messages.Length == 0)
				return;
			foreach (var message in messages)
			{
				var packed = PackToNewMessage(headers, message);
				TransactionalSendMessage(_queue, packed);
			}
		}

		public void RouteMessages(IncomingMessage[] messages, Action<NameValueCollection> headers)
		{
			foreach (var message in messages)
			{
				var packed = PackToNewMessage(headers, message.Message);
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

		CloudQueueMessage PackToNewMessage(Action<NameValueCollection> modify, object message)
		{
			// optimistic serialization for now
			var messageId = Guid.NewGuid();
			var referenceId = SystemUtil.UtcNow.ToString(DateFormatInBlobName) + messageId;

			var headers = new NameValueCollection();

			OutgoingMessageEnvelope
				.Build(e =>
					{
						e.Topic = message.GetType().FullName;
						e.Sender = _queue.Uri.ToString();
						e.Id = messageId;
					})
				.CopyHeaders(headers);
			modify(headers);

			var data = new MessageMessage(headers, message);
			using (var stream = new MemoryStream())
			{
				//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
				_messageSerializer.Serialize(data, stream);
				if (stream.Position < 6144)
				{
					var bytes = stream.ToArray();
					return new CloudQueueMessage(bytes);
				}
			}
			// overflow
			using (var stream = new MemoryStream())
			{
				_messageSerializer.Serialize(message, stream);
				_cloudBlob.GetBlobReference(referenceId).UploadByteArray(stream.ToArray());
			}

			headers["content-blob-ref"] = referenceId;

			headers["content-blob-type"] = _messageSerializer.GetContractNameByType(message.GetType());


			var data2 = new MessageMessage(headers, null);
			using (var stream = new MemoryStream())
			{
				_messageSerializer.Serialize(data2, stream);
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