using System;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class StatelessAzureFutureList
	{
		readonly ISystemObserver _observer;
		readonly IMessageSerializer _serializer;
		readonly CloudBlobContainer _container;
		

		public StatelessAzureFutureList(CloudStorageAccount account, string queueName, ISystemObserver observer, IMessageSerializer serializer)
		{
			_observer = observer;
			_serializer = serializer;
			_container = account.CreateCloudBlobClient().GetContainerReference(queueName);
		}

		public void PutMessage(MessageEnvelope envelope)
		{
			if (envelope.DeliverOn == default(DateTimeOffset))
				throw new InvalidOperationException();
			
		}

		public bool TakePendingMessage(out MessageEnvelope envelope)
		{
			
		}
	}
}