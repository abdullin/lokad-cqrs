#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Feature.AzureSender;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class AzurePartitionScheduler
	{
		readonly StatelessAzureQueueWriter[] _writers;
		readonly StatelessAzureFutureList[] _futures;

		public AzurePartitionScheduler(StatelessAzureQueueWriter[] writers, StatelessAzureFutureList[] futures)
		{
			_writers = writers;
			_futures = futures;
		}

		public void DispatchDelayedMessages()
		{
			for (int i = 0; i < _futures.Length; i++)
			{
				foreach (var message in _futures[i].TakePendingMessages())
				{
					_writers[i].PutMessage(message);
				}
			}
		}
	}
}