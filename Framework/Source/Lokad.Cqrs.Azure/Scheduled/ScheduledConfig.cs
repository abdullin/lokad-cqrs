#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Transactions;

namespace Lokad.Cqrs.Scheduled
{
	public sealed class ScheduledConfig
	{
		public IsolationLevel IsolationLevel { get; set; }
		public TimeSpan SleepBetweenCommands { get; set; }
		public TimeSpan SleepOnEmptyChain { get; set; }
		public TimeSpan SleepOnFailure { get; set; }

		public ScheduledConfig()
		{
			IsolationLevel = IsolationLevel.ReadCommitted;
			SleepBetweenCommands = 1.Seconds();
			SleepOnEmptyChain = 10.Seconds();
			SleepOnFailure = 10.Seconds();
		}
	}
}