#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Cqrs.Logging
{
	/// <summary>
	/// Shared interface to abstract away from the specific
	/// logging library
	/// </summary>
	public interface ILog
	{
		/// <summary> Writes the message to the logger </summary>
		/// <param name="level">The importance level</param>
		/// <param name="message">The actual message</param>
		void Log(LogLevel level, object message);

		/// <summary>
		/// Writes the exception and associated information 
		/// to the logger
		/// </summary>
		/// <param name="level">The importance level</param>
		/// <param name="ex">The actual exception</param>
		/// <param name="message">Information related to the exception</param>
		void Log(LogLevel level, Exception ex, object message);
	}
}