using Lokad.Cqrs.Durability;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Consume
{
	public sealed class AzureMessageContext
	{
		public readonly CloudQueueMessage CloudMessage;
		public readonly MessageEnvelope Unpacked;
		
		public AzureMessageContext(CloudQueueMessage cloudMessage, MessageEnvelope unpacked)
		{
			CloudMessage = cloudMessage;
			Unpacked = unpacked;
		}
	}
}