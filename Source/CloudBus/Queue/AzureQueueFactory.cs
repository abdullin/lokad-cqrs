using System.Collections.Generic;
using Bus2.Serialization;
using Lokad;
using Microsoft.WindowsAzure;

namespace Bus2.Queue
{
	public sealed class AzureQueueFactory : IQueueManager, IRouteMessages
	{
		CloudStorageAccount _account;
		IMessageSerializer _serializer;
		ILogProvider _logProvider;
		int _retryCount = 3;

		readonly IDictionary<string, AzureMessageQueue> _queues = new Dictionary<string, AzureMessageQueue>();

		public AzureQueueFactory(CloudStorageAccount account, IMessageSerializer serializer, ILogProvider logProvider)
		{
			_account = account;
			_serializer = serializer;
			_logProvider = logProvider;
		}

		public IReadMessageQueue GetReadQueue(string queueName)
		{
			lock (_queues)
			{
				return GetOrCreateQueue(queueName);
			}
		}

		public IWriteMessageQueue GetWriteQueue(string queueName)
		{
			lock (_queues)
			{
				return GetOrCreateQueue(queueName);
			}
		}

		AzureMessageQueue GetOrCreateQueue(string queueName)
		{
			AzureMessageQueue value;
			if (!_queues.TryGetValue(queueName, out value))
			{
				value = new AzureMessageQueue(_account, queueName, _retryCount, _logProvider, _serializer);
				value.Init();
				_queues.Add(queueName, value);
			}
			return value;
		}


		public void RouteMessages(IncomingMessage[] messages, params string[] references)
		{
			//var endpoints = references.Convert(AzureQueueReference.FromUri);

			//var unsupported = endpoints.Where(aqr => aqr.Uri != _account.QueueEndpoint).ToArray();

			//if (endpoints.Any())
			//{
			//    throw new InvalidOperationException("Can't send messages to unknown queues: " +
			//        unsupported.Select(s => s.Uri.ToString()).Join("; "));
			//}
			IWriteMessageQueue[] queues;
			lock (_queues)
			{
				queues = references.Convert(GetOrCreateQueue);
			}
			foreach (var queue in queues)
			{
				queue.RouteMessages(messages, nv => { });
			}
		}
	}
}