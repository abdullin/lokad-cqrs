#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary>
	/// Some helper shortcuts for the <see cref="double"/>
	/// </summary>
	public static class ExtendDouble
	{
		/// <summary>
		/// Rounds the specified double with the provided number 
		/// of fractional digits.
		/// </summary>
		/// <param name="value">The value to round.</param>
		/// <param name="digits">The digits.</param>
		/// <returns>rounded value</returns>
		public static double Round(this double value, int digits)
		{
			return Math.Round(value, digits);
		}


		/// <summary>Returns a <see cref="TimeSpan"/> that represents a specified number of minutes.</summary>
		/// <param name="minutes">number of minutes</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>3D.Minutes()</example>
		public static TimeSpan Minutes(this double minutes)
		{
			return TimeSpan.FromMinutes(minutes);
		}


		/// <summary>Returns a <see cref="TimeSpan"/> that represents a specified number of hours.</summary>
		/// <param name="hours">number of hours</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>3D.Hours()</example>
		public static TimeSpan Hours(this double hours)
		{
			return TimeSpan.FromHours(hours);
		}

		/// <summary>Returns a <see cref="TimeSpan"/> that represents a specified number of seconds.</summary>
		/// <param name="seconds">number of seconds</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		/// <example>2D.Seconds()</example>
		public static TimeSpan Seconds(this double seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		/// <summary>Returns a <see cref="TimeSpan"/> that represents a specified number of milliseconds.</summary>
		/// <param name="milliseconds">milliseconds for this timespan</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		public static TimeSpan Milliseconds(this double milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds);
		}

		/// <summary>
		/// Returns a <see cref="TimeSpan"/> that represents a specified number of days.
		/// </summary>
		/// <param name="days">Number of days, accurate to the milliseconds.</param>
		/// <returns>A <see cref="TimeSpan"/> that represents a value.</returns>
		public static TimeSpan Days(this double days)
		{
			return TimeSpan.FromDays(days);
		}
	}
}