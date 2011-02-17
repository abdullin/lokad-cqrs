#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;

namespace Lokad
{
	/// <summary>
	/// Helper extensions for any class that implements <see cref="ILog"/>
	/// </summary>
	public static class ExtendILog
	{
		static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		/// <summary>
		/// Writes message with <see cref="LogLevel.Debug"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="message">Message</param>
		public static void Debug(this ILog log, object message)
		{
			log.Log(LogLevel.Debug, message);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Debug"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void DebugFormat(this ILog log, string format, params object[] args)
		{
			log.Log(LogLevel.Debug, string.Format(Culture, format, args));
		}


		/// <summary>
		/// Writes message with <see cref="LogLevel.Debug"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void DebugFormat(this ILog log, Exception ex, string format, params object[] args)
		{
			log.Log(LogLevel.Debug, ex, string.Format(Culture, format, args));
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Info"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="message">Message</param>
		public static void Info(this ILog log, object message)
		{
			log.Log(LogLevel.Info, message);
		}







		/// <summary>
		/// Writes message with <see cref="LogLevel.Warn"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void WarnFormat(this ILog log, Exception ex, string format, params object[] args)
		{
			log.Log(LogLevel.Warn, ex, string.Format(Culture, format, args));
		}



		/// <summary>
		/// Writes message with <see cref="LogLevel.Error"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void ErrorFormat(this ILog log, string format, params object[] args)
		{
			log.Log(LogLevel.Error, string.Format(Culture, format, args));
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Error"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="message">Message</param>
		public static void Error(this ILog log, Exception ex, object message)
		{
			log.Log(LogLevel.Error, ex, message);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Error"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void ErrorFormat(this ILog log, Exception ex, string format, params object[] args)
		{
			log.Log(LogLevel.Error, ex, string.Format(Culture, format, args));
		}
	}
}