#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Queue;
using Lokad.Quality;

namespace Lokad.Cqrs
{
	public static class MessageContext
	{
		[ThreadStatic] static UnpackedMessage _current;

		public static UnpackedMessage Current { get { return _current; } }

		internal static void OverrideContext(UnpackedMessage message)
		{
			_current = message;
		}
	}

	[UsedImplicitly]
	sealed class NullEngineProfiler : IEngineProfiler
	{
		public static readonly IEngineProfiler Instance = new NullEngineProfiler();

		IDisposable IEngineProfiler.TrackMessage(UnpackedMessage message)
		{
			return null;
		}

		public IDisposable TrackContext(string context)
		{
			return null;
		}
	}
}