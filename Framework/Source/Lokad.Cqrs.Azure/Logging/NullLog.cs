#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Cqrs.Logging
{
	/// <summary>
	/// <see cref="ILog"/> that does not do anything
	/// </summary>
	[Serializable]
	public sealed class NullLog : ILog
	{
		public void Log(ILogEvent @event)
		{
			
		}
	}
}