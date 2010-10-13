#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

#if !SILVERLIGHT2
using System;
using System.Diagnostics;

namespace Lokad
{
	/// <summary>
	/// Simple <see cref="ILog"/> that writes to the <see cref="Trace.Listeners"/>
	/// </summary>
	/// <remarks>Use Logging stack, if more flexibility is needed</remarks>
	[Serializable]
	[NoCodeCoverage, UsedImplicitly]
	public sealed class TraceLog : ILog
	{
		/// <summary>  Singleton instance </summary>
		[UsedImplicitly] public static readonly ILog Instance = new TraceLog("");

		/// <summary>
		/// Named provider for the <see cref="TraceLog"/>
		/// </summary>
		[UsedImplicitly] public static readonly ILogProvider Provider =
			new LambdaLogProvider(s => new TraceLog(s));

		readonly string _logName;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceLog"/> class.
		/// </summary>
		/// <param name="logName">Name of the log.</param>
		public TraceLog(string logName)
		{
			_logName = logName;
		}


		void ILog.Log(LogLevel level, object message)
		{
			Trace.WriteLine("[" + level + "] " + message, _logName);
			Trace.Flush();
		}

		void ILog.Log(LogLevel level, Exception ex, object message)
		{
			Trace.WriteLine("[" + level + "] " + message, _logName);
			Trace.WriteLine("[" + level + "] " + ex, _logName);
			Trace.Flush();
		}

		bool ILog.IsEnabled(LogLevel level)
		{
			return true;
		}
	}
}

#endif