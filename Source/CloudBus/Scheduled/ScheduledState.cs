using System;
using Lokad;

namespace Bus2.Scheduled
{
	public sealed class ScheduledState
	{
		

		public TimeSpan Happen()
		{
			return Delegate();
		}

		public DateTime NextRun { get; private set; }
		public int ExceptionCount { get; private set; }
		public Maybe<Exception> LastException { get; private set; }

		public readonly string Name;
		public readonly Func<TimeSpan> Delegate;

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

		public ScheduledState(string name, Func<TimeSpan> @delegate)
		{
			
			NextRun = SystemUtil.UtcNow;
			LastException = Maybe<Exception>.Empty;
			Name = name;
			Delegate = @delegate;
		}
	}

	

	public sealed class ScheduledInfo
	{
		public readonly string Name;
		public readonly Func<TimeSpan> Delegate;

		public ScheduledInfo(string name, Func<TimeSpan> @delegate)
		{
			Name = name;
			Delegate = @delegate;
		}
	}
}