using System;

namespace FarleyFile.Engine.Design
{

        public sealed class MessageContext
    {
        public readonly string EnvelopeId;
        public readonly DateTime CreatedUtc;

        public MessageContext(string envelopeId, DateTime createdUtc)
        {
            EnvelopeId = envelopeId;
            CreatedUtc = createdUtc;
        }
    }

}
