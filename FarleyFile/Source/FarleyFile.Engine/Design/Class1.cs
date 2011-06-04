using System;

namespace Farley.Engine.Design
{

    public interface IConsumeMessage<TMessage> : IConsumeMessage
        where TMessage : FarleyFile.Farley.IMessage
    {
        void Consume(TMessage message);
    }

    public interface IConsumeMessage
    {
    }

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

    public interface Handle<TCommand> : IConsumeMessage<TCommand>
where TCommand : FarleyFile.Farley.Command
    {
    }

    public interface Consume<TEvent> : IConsumeMessage<TEvent>
where TEvent : FarleyFile.Farley.Event
    {
    }
}
