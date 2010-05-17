#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace CloudBus.Scheduled
{
	public sealed class ScheduledInfo
	{
		public readonly Func<TimeSpan> Delegate;
		public readonly string Name;

		public ScheduledInfo(string name, Func<TimeSpan> @delegate)
		{
			Name = name;
			Delegate = @delegate;
		}
	}
}