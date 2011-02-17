#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
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

		readonly IDictionary<string, AzureReadQueue> _readQueues = new Dictionary<string, AzureReadQueue>();
		readonly IMessageSerializer _serializer;
		readonly IDictionary<string, AzureWriteQueue> _writeQueues = new Dictionary<string, AzureWriteQueue>();

		public AzureQueueFactory(
			CloudStorageAccount account,
			IMessageSerializer serializer,
			ILogProvider logProvider)
		{
			_account = account;
			_serializer = serializer;
			_logProvider = logProvider;
		}


		public AzureReadQueue GetReadQueue(string queueName)
		{
			lock (_readQueues)
			{
				AzureReadQueue value;
				if (!_readQueues.TryGetValue(queueName, out value))
				{
					value = new AzureReadQueue(_account, queueName, _logProvider, _serializer);
					value.Init();
					_readQueues.Add(queueName, value);
				}
				return value;
			}
		}

		public AzureWriteQueue GetSendQueue(string queueName)
		{
			lock (_writeQueues)
			{
				AzureWriteQueue value;
				if (!_writeQueues.TryGetValue(queueName, out value))
				{
					value = new AzureWriteQueue(_serializer, _account, queueName);
					value.Init();
					_writeQueues.Add(queueName, value);
				}
				return value;
			}
		}
	}
}