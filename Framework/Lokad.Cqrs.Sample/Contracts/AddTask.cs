using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Sample.Contracts
{
    [DataContract]
    public sealed class AddTask : Define.Command
    {
        [DataMember]
        public readonly string Name;

        public AddTask(string name)
        {
            Name = name;
        }
    }
}