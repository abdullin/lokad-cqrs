using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Lokad.Cqrs.Scenarios.SimpleES.Contracts
{
    [DataContract]
    public sealed class CreateAccount : IAccountCommand
    {
        [DataMember]
        public readonly string Name;

        public CreateAccount(string name)
        {
            Name = name;
        }
    }
}