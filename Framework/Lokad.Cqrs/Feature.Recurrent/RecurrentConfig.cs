#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.Recurrent
{
	public sealed class RecurrentConfig
	{
		public TimeSpan SleepBetweenCommands { get; set; }
		public TimeSpan SleepOnEmptyChain { get; set; }
		public TimeSpan SleepOnFailure { get; set; }

		public RecurrentConfig()
		{
			SleepBetweenCommands = 1.Seconds();
			SleepOnEmptyChain = 10.Seconds();
			SleepOnFailure = 10.Seconds();
		}
	}
}