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