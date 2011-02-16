#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion


using System;
using System.Diagnostics;


namespace Lokad
{
	/// <summary>
	/// Simple <see cref="ILog"/> that writes to the <see cref="Trace.Listeners"/>, if the
	/// <em>DEBUG</em> symbol is defined
	/// </summary>
	/// <remarks>Use Logging stack, if more flexibility is needed</remarks>
	[Serializable]
	public sealed class DebugLog : ILog
	{
		/// <summary>  Singleton instance </summary>
		 public static readonly ILog Instance = new DebugLog("");

		/// <summary>
		/// Named provider for the <see cref="DebugLog"/>
		/// </summary>
		 public static readonly ILogProvider Provider =
			new LambdaLogProvider(s => new DebugLog(s));

		readonly string _logName;

		/// <summary>
		/// Initializes a new instance of the <see cref="DebugLog"/> class.
		/// </summary>
		/// <param name="logName">Name of the log.</param>
		public DebugLog(string logName)
		{
			_logName = logName;
		}

		void ILog.Log(LogLevel level, object message)
		{
			Debug.WriteLine("[" + level + "] " + message, _logName);
			Debug.Flush();
		}

		void ILog.Log(LogLevel level, Exception ex, object message)
		{
			Debug.WriteLine("[" + level + "] " + message, _logName);
			Debug.WriteLine("[" + level + "] " + ex, _logName);
			Debug.Flush();
		}

		bool ILog.IsEnabled(LogLevel level)
		{
			return true;
		}
	}
}