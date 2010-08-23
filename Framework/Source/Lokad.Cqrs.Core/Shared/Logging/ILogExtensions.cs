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
	public static class ILogExtensions
	{
		static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

		/// <summary>
		/// Determines whether the specified log is recording debug messages.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <returns>
		/// 	<c>true</c> if the specified log is recording debug messages; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsDebugEnabled(this ILog log)
		{
			return log.IsEnabled(LogLevel.Debug);
		}


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
			log.Log(LogLevel.Debug, string.Format(_culture, format, args));
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Debug"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="message">Message</param>
		public static void Debug(this ILog log, Exception ex, object message)
		{
			log.Log(LogLevel.Debug, ex, message);
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
			log.Log(LogLevel.Debug, ex, string.Format(_culture, format, args));
		}

		/// <summary>
		/// Determines whether the specified log is recording info messages.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <returns>
		/// 	<c>true</c> if the specified log is recording info messages; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInfoEnabled(this ILog log)
		{
			return log.IsEnabled(LogLevel.Info);
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
		/// Writes message with <see cref="LogLevel.Info"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void InfoFormat(this ILog log, string format, params object[] args)
		{
			log.Log(LogLevel.Info, string.Format(_culture, format, args));
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Info"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="message">Message</param>
		public static void Info(this ILog log, Exception ex, object message)
		{
			log.Log(LogLevel.Info, ex, message);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Info"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void InfoFormat(this ILog log, Exception ex, string format, params object[] args)
		{
			log.Log(LogLevel.Info, ex, string.Format(_culture, format, args));
		}


		/// <summary>
		/// Determines whether the specified log is recording warning messages.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <returns>
		/// 	<c>true</c> if the specified log is recording warning messages; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsWarnEnabled(this ILog log)
		{
			return log.IsEnabled(LogLevel.Warn);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Warn"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="message">Message</param>
		public static void Warn(this ILog log, object message)
		{
			log.Log(LogLevel.Warn, message);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Warn"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void WarnFormat(this ILog log, string format, params object[] args)
		{
			log.Log(LogLevel.Warn, string.Format(_culture, format, args));
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Warn"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="message">Message</param>
		public static void Warn(this ILog log, Exception ex, object message)
		{
			log.Log(LogLevel.Warn, ex, message);
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
			log.Log(LogLevel.Warn, ex, string.Format(_culture, format, args));
		}


		/// <summary>
		/// Determines whether the specified log is recording error messages.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <returns>
		/// 	<c>true</c> if the specified log is recording error messages; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsErrorEnabled(this ILog log)
		{
			return log.IsEnabled(LogLevel.Error);
		}


		/// <summary>
		/// Writes message with <see cref="LogLevel.Error"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="message">Message</param>
		public static void Error(this ILog log, object message)
		{
			log.Log(LogLevel.Error, message);
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
			log.Log(LogLevel.Error, string.Format(_culture, format, args));
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
			log.Log(LogLevel.Error, ex, string.Format(_culture, format, args));
		}

		/// <summary>
		/// Determines whether the specified log is recording Fatal messages.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <returns>
		/// 	<c>true</c> if the specified log is recording datal messages; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsFatalEnabled(this ILog log)
		{
			return log.IsEnabled(LogLevel.Fatal);
		}


		/// <summary>
		/// Writes message with <see cref="LogLevel.Fatal"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="message">Message</param>
		public static void Fatal(this ILog log, object message)
		{
			log.Log(LogLevel.Fatal, message);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Fatal"/> level
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void FatalFormat(this ILog log, string format, params object[] args)
		{
			log.Log(LogLevel.Fatal, string.Format(_culture, format, args));
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Fatal"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="message">Message</param>
		public static void Fatal(this ILog log, Exception ex, object message)
		{
			log.Log(LogLevel.Fatal, ex, message);
		}

		/// <summary>
		/// Writes message with <see cref="LogLevel.Fatal"/> level and
		/// appends the specified <see cref="Exception"/>
		/// </summary>
		/// <param name="log">Log instance being extended</param>
		/// <param name="ex">Exception to add to the message</param>
		/// <param name="format">Format string as in 
		/// <see cref="string.Format(string,object[])"/></param>
		/// <param name="args">Arguments</param>
		public static void FatalFormat(this ILog log, Exception ex, string format, params object[] args)
		{
			log.Log(LogLevel.Fatal, ex, string.Format(_culture, format, args));
		}
	}
}