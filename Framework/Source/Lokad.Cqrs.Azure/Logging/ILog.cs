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
		void Log(ILogEvent @event);
	}


	public interface ILogEvent
	{
		
	}
}