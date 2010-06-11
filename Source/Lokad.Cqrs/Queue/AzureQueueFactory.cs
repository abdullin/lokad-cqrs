#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Serialization;
using Lokad.Quality;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Queue
{
	[UsedImplicitly]
	public sealed class AzureQueueFactory : IQueueManager, IRouteMessages
	{
		const int RetryCount = 4;
		readonly CloudStorageAccount _account;
		readonly ILogProvider _logProvider;
		readonly IMessageProfiler _profiler;

		readonly IDictionary<string, AzureMessageQueue> _queues = new Dictionary<string, AzureMessageQueue>();
		readonly IMessageSerializer _serializer;

		public AzureQueueFactory(
			CloudStorageAccount account,
			IMessageSerializer serializer,
			ILogProvider logProvider,
			IMessageProfiler profiler)
		{
			_account = account;
			_serializer = serializer;
			_logProvider = logProvider;
			_profiler = profiler;
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

		public string[] GetQueueNames()
		{
			lock (_queues)
			{
				return _queues.ToArray(c => c.Key);
			}
		}

		AzureMessageQueue GetOrCreateQueue(string queueName)
		{
			AzureMessageQueue value;
			if (!_queues.TryGetValue(queueName, out value))
			{
				value = new AzureMessageQueue(_account, queueName, RetryCount, _logProvider, _serializer, _profiler);
				value.Init();
				_queues.Add(queueName, value);
			}
			return value;
		}
	}
}