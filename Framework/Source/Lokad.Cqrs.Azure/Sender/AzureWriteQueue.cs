using System;
using Lokad.Cqrs.Durability;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Sender
{
	public sealed class AzureWriteQueue
	{
		public void SendMessages(object[] messages)
		{
			if (messages.Length == 0)
				return;

			var packed = PackNewMessage(messages);
			_queue.AddMessage(packed);
		}

		//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
		const int CloudQueueLimit = 6144;


		CloudQueueMessage PackNewMessage(object[] items)
		{
			var messageId = Guid.NewGuid().ToString().ToLowerInvariant();

			var builder = new MessageEnvelopeBuilder(messageId);
			foreach (var item in items)
			{
				builder.AddItem(item);
			}
			var created = DateTimeOffset.UtcNow;
			builder.Attributes.Add(MessageAttributes.Envelope.CreatedUtc, created);
			var buffer = MessageUtil.SaveDataMessage(builder, _serializer);
			if (buffer.Length < CloudQueueLimit)
			{
				// write message to queue
				return new CloudQueueMessage(buffer);
			}


			// ok, we didn't fit, so create reference message
			var referenceId = created.ToString(DateFormatInBlobName) + "-" + messageId;
			_cloudBlob.GetBlobReference(referenceId).UploadByteArray(buffer);
			var reference = new MessageReference(messageId.ToString(), _cloudBlob.Uri.ToString(), referenceId);
			var blob = MessageUtil.SaveReferenceMessage(reference);
			return new CloudQueueMessage(blob);
		}

		public AzureWriteQueue(IMessageSerializer serializer, CloudStorageAccount account, string queueName)
		{
			_serializer = serializer;
			var blobClient = account.CreateCloudBlobClient();
			blobClient.RetryPolicy = RetryPolicies.NoRetry();

			_cloudBlob = blobClient.GetContainerReference(queueName);

			var queueClient = account.CreateCloudQueueClient();
			queueClient.RetryPolicy = RetryPolicies.NoRetry();
			_queue = queueClient.GetQueueReference(queueName);

		}

		public void Init()
		{
			_queue.CreateIfNotExist();
			_cloudBlob.CreateIfNotExist();
		}


		const string DateFormatInBlobName = "yyyy-MM-dd-HH-mm-ss-ffff";
		readonly IMessageSerializer _serializer;
		readonly CloudBlobContainer _cloudBlob;
		readonly CloudQueue _queue;

		public static Exception NoContractNameOnSend(Type messageType, IMessageSerializer serializer)
		{
			return Errors.InvalidOperation(
				"Can't find contract name to serialize message: '{0}'. Make sure that your message types are loaded by domain and are compatible with '{1}'.",
				messageType, serializer.GetType().Name);
		}
	}
}