using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Lokad.Cqrs.Scenarios.SimpleES.Contracts
{
    [DataContract]
    public sealed class AddLogin : IAccountCommand
    {
        [DataMember]
        public readonly string Username;
        [DataMember]
        public readonly string Password;

        public AddLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}