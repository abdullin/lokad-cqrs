#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using System.Linq;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
	public sealed class AzureWriteQueueFactory : IQueueWriterFactory
	{
		readonly CloudStorageAccount[] _accounts;
		readonly IEnvelopeStreamer _streamer;
		
		readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues = new ConcurrentDictionary<string, IQueueWriter>();

		public AzureWriteQueueFactory(
			IEnumerable<CloudStorageAccount> accounts,
			IEnvelopeStreamer streamer)
		{
			_accounts = accounts.ToArray();
			_streamer = streamer;
		}


		public bool TryGetWriteQueue(string queueName, out IQueueWriter writer)
		{
			foreach (var account in _accounts)
			{
				var accountName = account.Credentials.AccountName;
				if (accountName == CloudStorageAccount.DevelopmentStorageAccount.Credentials.AccountName)
				{
					accountName = "azure-dev";
				}
				var match = accountName + ":";
				if (queueName.StartsWith(match, StringComparison.InvariantCultureIgnoreCase))
				{
					var cleanedName = queueName.Remove(0, match.Length).TrimStart();

					writer = _writeQueues.GetOrAdd(queueName, name =>
					{
						var v = new StatelessAzureQueueWriter(_streamer, account, cleanedName);
						v.Init();
						return v;
					});
					return true;
				}
			}

			writer = null;
			return false;
		}
	}
}