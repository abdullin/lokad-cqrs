#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;

using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Queue
{
	
	public sealed class AzureQueueFactory
	{
		readonly CloudStorageAccount _account;
		readonly ILogProvider _logProvider;

		readonly IDictionary<string, AzureMessageQueue> _queues = new Dictionary<string, AzureMessageQueue>();
		readonly IMessageSerializer _serializer;

		public AzureQueueFactory(
			CloudStorageAccount account,
			IMessageSerializer serializer,
			ILogProvider logProvider)
		{
			_account = account;
			_serializer = serializer;
			_logProvider = logProvider;
		}

		

		public AzureMessageQueue GetQueue(string queueName)
		{
			lock (_queues)
			{
				AzureMessageQueue value;
				if (!_queues.TryGetValue(queueName, out value))
				{
				
					value = new AzureMessageQueue(_account, queueName, _logProvider, _serializer);
					value.Init();
					_queues.Add(queueName, value);
				}
				return value;
			}
		}
	}
}