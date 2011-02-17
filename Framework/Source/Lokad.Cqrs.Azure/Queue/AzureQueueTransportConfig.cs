#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Transport
{
	public sealed class AzureQueueTransportConfig
	{
		public readonly string[] QueueNames;
		public readonly Func<uint, TimeSpan> SleepWhenNoMessages;


		public AzureQueueTransportConfig(string[] queueNames,
			Func<uint, TimeSpan> sleepWhenNoMessages)
		{
			
			SleepWhenNoMessages = sleepWhenNoMessages;
			QueueNames = queueNames;
		}
	}
}