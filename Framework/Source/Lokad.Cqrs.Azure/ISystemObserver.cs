#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Cqrs
{
	/// <summary>
	/// Sends notification to the system. This is a strongly-typed equivalent of logging
	/// </summary>
	public interface ISystemObserver
	{
		/// <summary>
		/// Notifies the observer about the specified @event.
		/// </summary>
		/// <param name="event">The @event.</param>
		void Notify(ISystemEvent @event);
	}
}