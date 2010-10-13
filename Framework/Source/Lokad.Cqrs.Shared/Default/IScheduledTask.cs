#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Default
{
	/// <summary>
	/// Default interface for use by the TaskScheduler feature
	/// </summary>
	/// <remarks>See http://code.google.com/p/lokad-cqrs/wiki/ScheduledTasks</remarks>
	public interface IScheduledTask
	{
		/// <summary>
		/// Executes some event that happens at the predefined moments of time.
		/// Each worker instance will have an independent scheduler.
		/// </summary>
		/// <returns>Amount of time to sleep till the next run</returns>
		TimeSpan Execute();
	}
}