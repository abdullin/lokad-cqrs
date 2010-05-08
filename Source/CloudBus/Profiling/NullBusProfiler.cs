#region Copyright (c) 2006-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2006-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;

namespace Bus2.Profiling
{
	public sealed class NullBusProfiler : IBusProfiler
	{
		public static readonly IBusProfiler Instance = new NullBusProfiler();

		public IDisposable TrackContext(object context)
		{
			return null;
		}
	}
}