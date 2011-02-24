
using System;

namespace Lokad.Cqrs.Extensions
{
	/// <summary> Extensions to the <see cref="int"/> </summary>
	public static class ExtendInt32
	{
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
	}
}