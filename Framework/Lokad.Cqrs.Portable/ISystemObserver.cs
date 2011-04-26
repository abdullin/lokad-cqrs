#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

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