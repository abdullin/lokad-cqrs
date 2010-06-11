#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Quality;

namespace Lokad.Cqrs
{
	[UsedImplicitly]
	sealed class NullEngineProfiler : IEngineProfiler
	{
		public static readonly IEngineProfiler Instance = new NullEngineProfiler();

		IDisposable IEngineProfiler.TrackMessage(object instance, string messageId)
		{
			return null;
		}

		public IDisposable TrackContext(string context)
		{
			return null;
		}
	}
}