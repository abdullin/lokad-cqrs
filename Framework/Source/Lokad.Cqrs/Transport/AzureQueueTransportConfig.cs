#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Transactions;

namespace Lokad.Cqrs.Transport
{
	public sealed class AzureQueueTransportConfig
	{
		public readonly IsolationLevel IsolationLevel;
		public readonly string LogName;
		public readonly string[] QueueNames;
		// TODO: replace with the thread management policy
		public readonly Func<uint, TimeSpan> SleepWhenNoMessages;
		public readonly int DegreeOfParallelism;


		public AzureQueueTransportConfig(string logName, int degreeOfParallelism, IsolationLevel isolationLevel, string[] queueNames,
			Func<uint, TimeSpan> sleepWhenNoMessages)
		{
			DegreeOfParallelism = degreeOfParallelism;
			LogName = logName;
			SleepWhenNoMessages = sleepWhenNoMessages;
			IsolationLevel = isolationLevel;
			QueueNames = queueNames;
		}
	}
}