using System;
using System.Transactions;

namespace Bus2.Transport
{
	public sealed class AzureQueueTransportConfig
	{
		public readonly int ThreadCount;
		public readonly IsolationLevel IsolationLevel;
		public readonly string[] QueueNames;
		// TODO: replace with the thread management policy
		public readonly TimeSpan SleepWhenNoMessages;



		public AzureQueueTransportConfig(
			int threadCount,
			IsolationLevel isolationLevel,
			string[] queueNames, 
			TimeSpan sleepWhenNoMessages)
		{
			ThreadCount = threadCount;
			SleepWhenNoMessages = sleepWhenNoMessages;
			IsolationLevel = isolationLevel;
			QueueNames = queueNames;
		}
	}
}