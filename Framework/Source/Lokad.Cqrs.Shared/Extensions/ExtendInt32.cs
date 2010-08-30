#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary> Extensions to the <see cref="int"/> </summary>
	public static class ExtendInt32
	{
		/// <summary>
		/// Returns kilobytes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Kb(this int value)
		{
			return value*1024;
		}

		/// <summary>
		/// Returns megabytes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Mb(this int value)
		{
			return value*1024*1024;
		}

		/// <summary>Returns a <see cref="TimeSpan"/> that represents a specified number of minutes.</summary>
		/// <param name="minutes">number of minutes</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>3.Minutes()</example>
		public static TimeSpan Minutes(this int minutes)
		{
			return TimeSpan.FromMinutes(minutes);
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of seconds.
		/// </summary>
		/// <param name="seconds">number of seconds</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>2.Seconds()</example>
		public static TimeSpan Seconds(this int seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of milliseconds.
		/// </summary>
		/// <param name="milliseconds">milliseconds for this timespan</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		public static TimeSpan Milliseconds(this int milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds);
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of days.
		/// </summary>
		/// <param name="days">Number of days.</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		public static TimeSpan Days(this int days)
		{
			return TimeSpan.FromDays(days);
		}


		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of hours.
		/// </summary>
		/// <param name="hours">Number of hours.</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		public static TimeSpan Hours(this int hours)
		{
			return TimeSpan.FromHours(hours);
		}
	}
}