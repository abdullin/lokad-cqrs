#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.Send
{
	public sealed class AzureWriteQueueFactory
	{
		readonly CloudStorageAccount _account;
		readonly IMessageSerializer _serializer;
		
		readonly IDictionary<string, IWriteQueue> _writeQueues = new Dictionary<string, IWriteQueue>();

		public AzureWriteQueueFactory(
			CloudStorageAccount account,
			IMessageSerializer serializer)
		{
			_account = account;
			_serializer = serializer;
		}


		public IWriteQueue GetWriteQueue(string queueName)
		{
			lock (_writeQueues)
			{
				IWriteQueue value;
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