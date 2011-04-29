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
        void DispatchMessage(ImmutableMessageEnvelope message);
        void Init();
    }
}