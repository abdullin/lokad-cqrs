using System;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition
{
	public sealed class StatelessAzureQueueWriter : IQueueWriter
	{
		public void PutMessage(MessageEnvelope envelope)
		{
			var packed = PrepareCloudMessage(envelope);
			_queue.AddMessage(packed);
		}

		//http://abdullin.com/journal/2010/6/4/azure-queue-messages-cannot-be-larger-than-8192-bytes.html
		const int CloudQueueLimit = 6144;


		CloudQueueMessage PrepareCloudMessage(MessageEnvelope builder)
		{

			var buffer = _serializer.SaveDataMessage(builder);
			if (buffer.Length < CloudQueueLimit)
			{
				// write message to queue
				return new CloudQueueMessage(buffer);
			}
			// ok, we didn't fit, so create reference message
			var referenceId = DateTimeOffset.UtcNow.ToString(DateFormatInBlobName) + "-" + builder.EnvelopeId;
			_cloudBlob.GetBlobReference(referenceId).UploadByteArray(buffer);
			var reference = new MessageReference(builder.EnvelopeId, _cloudBlob.Uri.ToString(), referenceId);
			var blob = _serializer.SaveReferenceMessage(reference);
			return new CloudQueueMessage(blob);
		}

		public StatelessAzureQueueWriter(IEnvelopeSerializer serializer, CloudStorageAccount account, string queueName)
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
		readonly IEnvelopeSerializer _serializer;
		readonly CloudBlobContainer _cloudBlob;
		readonly CloudQueue _queue;
	}
}