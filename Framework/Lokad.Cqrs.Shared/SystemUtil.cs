#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Threading;

namespace Lokad
{
	/// <summary>
	/// System utils to improve testability of the code
	/// </summary>
	public static class SystemUtil
	{
		/// <summary>
		/// <see cref="Thread.Sleep(TimeSpan)"/>
		/// </summary>
		static Action<TimeSpan> SleepAction;

		/// <summary>
		/// Allows to set custom date time implementation for the testing purposes.
		/// </summary>
		static Func<DateTime> DateTimeProvider;

		/// <summary>
		/// <see cref="DateTime.Now"/>
		/// </summary>
		public static DateTime Now
		{
			get { return DateTimeProvider(); }
		}

		/// <summary>
		/// <see cref="DateTime.UtcNow"/>
		/// </summary>
		public static DateTime UtcNow
		{
			get { return Now.ToUniversalTime(); }
		}

		/// <summary>
		/// Unambiguous date and time with UTC offset, <see cref="DateTimeOffset.Now"/>.
		/// </summary>
		public static DateTimeOffset NowOffset
		{
			get { return new DateTimeOffset(Now); }
		}

		static SystemUtil()
		{
			Reset();
		}

		/// <summary>
		/// Returns all overridable functions to default. To be used by test teardowns
		/// </summary>
		public static void Reset()
		{
			DateTimeProvider = () => DateTime.Now;
			SleepAction = span => Thread.Sleep(span);
		}

		/// <summary>
		/// Sets the custom sleep routine.
		/// </summary>
		/// <param name="sleepRoutine">The sleep routine.</param>
		public static void SetSleep(Action<TimeSpan> sleepRoutine)
		{
			SleepAction = sleepRoutine;
		}

		/// <summary>
		/// Sets the custom date time provider routine.
		/// </summary>
		/// <param name="dateTimeProvider">The date time provider.</param>
		public static void SetDateTimeProvider(Func<DateTime> dateTimeProvider)
		{
			DateTimeProvider = dateTimeProvider;
		}

		/// <summary>
		/// Shortcut to set the custom date time.
		/// </summary>
		/// <param name="time">The time.</param>
		public static void SetTime(DateTime time)
		{
			DateTimeProvider = () => time;
		}


		/// <summary>
		/// Invokes the method associated with sleeping. For the production purposes
		/// this should be a call to <see cref="Thread.Sleep(TimeSpan)"/>
		/// </summary>
		/// <param name="span">The span.</param>
		public static void Sleep(TimeSpan span)
		{
			SleepAction(span);
		}

		internal static int GetHashCode(params object[] args)
		{
			unchecked
			{
				int result = 0;
				foreach (var o in args)
				{
					result = (result*397) ^ (o !=null ? o.GetHashCode() : 0);
				}
				return result;
			}
		}
	}
}