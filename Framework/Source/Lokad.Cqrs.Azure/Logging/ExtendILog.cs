#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;

namespace Lokad.Cqrs.Logging
{
	/// <summary>
	/// Helper extensions for any class that implements <see cref="ILog"/>
	/// </summary>
	public static class ExtendILog
	{
		static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		public static void Debug(this ILog log, object message)
		{
			log.Log(LogLevel.Debug, message);
		}
		
		public static void DebugFormat(this ILog log, string format, params object[] args)
		{
			log.Log(LogLevel.Debug, string.Format(Culture, format, args));
		}


		public static void DebugFormat(this ILog log, Exception ex, string format, params object[] args)
		{
			log.Log(LogLevel.Debug, ex, string.Format(Culture, format, args));
		}
	}
}