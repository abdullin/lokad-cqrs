using System;
using System.IO;
using Lokad.Cqrs.Lmf;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Queue
{
	public sealed class AzureWriteQueue
	{
		public void SendMessages(object[] messages)
		{
			if (messages.Length == 0)
				return;
			foreach (var message in messages)
			{
				var packed = PackNewMessage(message);
				_queue.AddMessage(packed);
			}
		}

		//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
		const int CloudQueueLimit = 6144;


		CloudQueueMessage PackNewMessage(object message)
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

	}
}