using System;

namespace Lokad.Cqrs
{
    public sealed class MessageContext
    {
        public readonly string EnvelopeId;
        public readonly DateTimeOffset CreatedUtc;

        public MessageContext(string envelopeId, DateTimeOffset createdUtc)
        {
            EnvelopeId = envelopeId;
            CreatedUtc = createdUtc;
        }
    }
}