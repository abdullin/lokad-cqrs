using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class AzureReadQueueFactory : IReadQueueFactory
	{
		readonly CloudStorageAccount _account;
		readonly IMessageSerializer _serializer;
		readonly ISystemObserver _observer;

		public AzureReadQueueFactory(CloudStorageAccount account, IMessageSerializer serializer, ISystemObserver observer)
		{
			_account = account;
			_serializer = serializer;
			_observer = observer;
		}

		public IReadQueue GetReadQueue(string name)
		{
			return new AzureReadQueue(_account, name, _observer, _serializer);
		}
	}
}