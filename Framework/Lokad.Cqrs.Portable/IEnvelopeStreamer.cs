#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs
{
    public interface IEnvelopeStreamer
    {
        byte[] SaveEnvelopeReference(EnvelopeReference reference);
        byte[] SaveEnvelopeData(ImmutableEnvelope envelope);
        bool TryReadAsEnvelopeReference(byte[] buffer, out EnvelopeReference reference);
        ImmutableEnvelope ReadAsEnvelopeData(byte[] buffer);
    }
}