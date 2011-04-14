using System.Threading;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	/// <summary>
	/// Handles deserialization, joins multiple queues and notifications
	/// </summary>
	public sealed class AzureReadNotifier
	{
		readonly AzureReadQueue[] _azureQueues;

		public AzureReadNotifier(AzureReadQueue[] azureQueues)
		{
			_azureQueues = azureQueues;
		}

		public void Init()
		{
			foreach (var queue in _azureQueues)
			{
				queue.Init();
			}
		}

		public void AckMessage(MessageContext message)
		{
			foreach (var queue in _azureQueues)
			{
				if (queue.Name == message.QueueName)
				{
					queue.AckMessage(message);
				}
			}
		}

	
	

		
		public void TryNotifyNack(MessageContext context)
		{
		}

		public bool TryGetMessage(CancellationToken token, out MessageContext context)
		{
			while(!token.IsCancellationRequested)
			{
				foreach (var queue in _azureQueues)
				{
					queue.TryGetMessage()
				}
			}


		}

	}
}