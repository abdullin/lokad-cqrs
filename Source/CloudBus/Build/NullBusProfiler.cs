#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Quality;

namespace CloudBus.Build
{
	[UsedImplicitly]
	sealed class NullBusProfiler : IBusProfiler
	{
		public static readonly IBusProfiler Instance = new NullBusProfiler();

		IDisposable IBusProfiler.TrackMessage(object instance, string messageId)
		{
			return null;
		}

		public IDisposable TrackContext(string context)
		{
			return null;
		}
	}
}