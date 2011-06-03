using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Sample.Contracts
{
    [DataContract]
    public sealed class AddTaskCommand : Define.Command
    {
        [DataMember]
        public string Name;
    }

    

    public sealed class AddTask : Define.Handle<AddTaskCommand>
    {
        public void Consume(AddTaskCommand message)
        {
            throw new NotImplementedException();
        }
    }
}