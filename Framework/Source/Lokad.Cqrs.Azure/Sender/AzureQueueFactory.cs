#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Sender
{
	public sealed class AzureQueueFactory
	{
		readonly CloudStorageAccount _account;
		readonly IMessageSerializer _serializer;
		
		readonly IDictionary<string, AzureWriteQueue> _writeQueues = new Dictionary<string, AzureWriteQueue>();

		public AzureQueueFactory(
			CloudStorageAccount account,
			IMessageSerializer serializer)
		{
			_account = account;
			_serializer = serializer;
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