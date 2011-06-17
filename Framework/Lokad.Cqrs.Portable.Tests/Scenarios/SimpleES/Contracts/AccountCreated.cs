using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Lokad.Cqrs.Scenarios.SimpleES.Contracts
{
    [DataContract]
    public sealed class AccountCreated : IAccountEvent
    {
        public readonly string Name;

        public AccountCreated(string name)
        {
            Name = name;
        }
    }
}