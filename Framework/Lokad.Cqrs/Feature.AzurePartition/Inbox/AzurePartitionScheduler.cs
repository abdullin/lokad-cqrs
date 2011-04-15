#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
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
				var writer = _writers[i];
				_futures[i].TranferPendingMessages(writer.PutMessage);
			}
		}
	}
}