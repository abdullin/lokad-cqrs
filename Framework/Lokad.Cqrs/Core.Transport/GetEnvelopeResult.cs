#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Core.Transport
{
    public sealed class GetEnvelopeResult
    {
        public static readonly GetEnvelopeResult Empty = new GetEnvelopeResult(null, GetEnvelopeResultState.Empty);
        public static readonly GetEnvelopeResult Retry = new GetEnvelopeResult(null, GetEnvelopeResultState.Retry);
        public readonly GetEnvelopeResultState State;
        readonly EnvelopeTransportContext _envelope;

        GetEnvelopeResult(EnvelopeTransportContext envelope, GetEnvelopeResultState state)
        {
            _envelope = envelope;
            State = state;
        }


        public EnvelopeTransportContext Envelope
        {
            get
            {
                if (State != GetEnvelopeResultState.Success)
                    throw new InvalidOperationException("State should be in success");
                return _envelope;
            }
        }

        public static GetEnvelopeResult Success(EnvelopeTransportContext envelope)
        {
            return new GetEnvelopeResult(envelope, GetEnvelopeResultState.Success);
        }

        public static GetEnvelopeResult Error()
        {
            return new GetEnvelopeResult(null, GetEnvelopeResultState.Exception);
        }
    }
}