#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad;

namespace CloudBus.Scheduled
{
	public sealed class ScheduledState
	{
		public readonly Func<TimeSpan> Delegate;
		public readonly string Name;

		public ScheduledState(string name, Func<TimeSpan> @delegate)
		{
			NextRun = SystemUtil.UtcNow;
			LastException = Maybe<Exception>.Empty;
			Name = name;
			Delegate = @delegate;
		}

		public DateTime NextRun { get; private set; }
		public int ExceptionCount { get; private set; }
		public Maybe<Exception> LastException { get; private set; }

		public TimeSpan Happen()
		{
			return Delegate();
		}

		public void Completed()
		{
			NextRun = SystemUtil.UtcNow;
		}

		public void RecordException(Exception exception)
		{
			LastException = exception;
			ExceptionCount += 1;
		}

		public void ScheduleIn(TimeSpan span)
		{
			NextRun = SystemUtil.UtcNow + span;
		}
	}
}