#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
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