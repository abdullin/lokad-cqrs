#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
	public sealed class AzureWriteQueueFactory : IWriteQueueFactory
	{
		readonly CloudStorageAccount _account;
		readonly IEnvelopeSerializer _serializer;
		
		readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues = new ConcurrentDictionary<string, IQueueWriter>();

		public AzureWriteQueueFactory(
			CloudStorageAccount account,
			IEnvelopeSerializer serializer)
		{
			_account = account;
			_serializer = serializer;
		}


		public IQueueWriter GetWriteQueue(string queueName)
		{
			return _writeQueues.GetOrAdd(queueName, name =>
				{
					var v = new StatelessAzureQueueWriter(_serializer, _account, queueName);
					v.Init();
					return v;
				});
		}
	}
}