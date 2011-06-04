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
    public sealed class AddTask : Farley.Command
    {
        [DataMember]
        public readonly string Name;

        public AddTask(string name)
        {
            Name = name;
        }
    }
}