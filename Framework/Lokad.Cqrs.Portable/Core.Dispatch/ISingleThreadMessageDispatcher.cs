#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Core.Dispatch
{
    /// <summary>
    /// Generic message dispatch interface
    /// </summary>
    public interface ISingleThreadMessageDispatcher
    {
        /// <summary>
        /// Dispatches the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void DispatchMessage(ImmutableEnvelope message);
        /// <summary>
        /// Initializes this instance
        /// </summary>
        void Init();
    }
}