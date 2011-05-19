using System;

namespace Lokad.Cqrs
{
    public sealed class MessageContext
    {
        public readonly string EnvelopeId;
        public readonly int MessageIndex;
        public readonly DateTime CreatedUtc;

        public MessageContext(string envelopeId, int messageIndex, DateTime createdUtc)
        {
            EnvelopeId = envelopeId;
            MessageIndex = messageIndex;
            CreatedUtc = createdUtc;
        }
    }
}