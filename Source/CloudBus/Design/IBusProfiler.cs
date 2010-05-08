using System;

namespace Bus2
{
	public interface IBusProfiler
	{
		IDisposable TrackContext(object context);
	}
}
