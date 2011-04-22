#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lokad.Cqrs.Feature.Logging
{
	/// <summary>
	/// Simple <see cref="ISystemObserver"/> that writes to the <see cref="Trace.Listeners"/>
	/// </summary>
	/// <remarks>Use Logging stack, if more flexibility is needed</remarks>
	[Serializable]
	
	public sealed class TraceSystemObserver : ISystemObserver
	{
		public void Notify(ISystemEvent @event)
		{
			Trace.WriteLine(@event);
			Trace.Flush();
		}
	}
}