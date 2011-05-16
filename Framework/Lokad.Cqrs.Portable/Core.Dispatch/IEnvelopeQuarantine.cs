#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Core.Dispatch
{
    public interface IEnvelopeQuarantine
    {
        bool TryToQuarantine(EnvelopeTransportContext context, Exception ex);
        void TryRelease(EnvelopeTransportContext context);
    }
}