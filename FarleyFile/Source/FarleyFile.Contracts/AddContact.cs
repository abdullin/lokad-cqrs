using System.Runtime.Serialization;

namespace FarleyFile
{
    public static class Farley
    {
        public interface IMessage
        {
        }

        public interface Event : IMessage
        {
        }
        public interface Command : IMessage
        {
        }


        public interface IFarleyHandler<TMessage> : IFarleyHandler
        where TMessage : Farley.IMessage
        {
            void Consume(TMessage message);
        }

        public interface IFarleyHandler
        {
        }

            public interface Handle<TCommand> : IFarleyHandler<TCommand>
where TCommand : FarleyFile.Farley.Command
    {
    }

    public interface Consume<TEvent> : IFarleyHandler<TEvent>
where TEvent : FarleyFile.Farley.Event
    {
    }



    }
    


    [DataContract]
    public sealed class AddContact : Farley.Command
    {
        [DataMember(Order = 1)]
        public readonly string Name;

        public AddContact(string name)
        {
            Name = name;
        }
    }
    [DataContract]
    public sealed class ContactAdded : Farley.Event
    {
        [DataMember(Order = 1)]
        public readonly string Name;

        public ContactAdded(string name)
        {
            Name = name;
        }
    }
}