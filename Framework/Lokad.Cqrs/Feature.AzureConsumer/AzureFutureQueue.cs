using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class AzureFutureQueue
	{
		readonly ISystemObserver _observer;
		readonly IMessageSerializer _serializer;
		readonly CloudBlobContainer _container;

		public AzureFutureQueue(CloudStorageAccount account, string queueName, ISystemObserver observer, IMessageSerializer serializer)
		{
			_observer = observer;
			_serializer = serializer;
			_container = container;
		}

		public void PutMessage(MessageEnvelope envelope)
		{
			
		}

		public bool TakePendingMessage(out MessageEnvelope envelope)
		{
			
		}
	}
}