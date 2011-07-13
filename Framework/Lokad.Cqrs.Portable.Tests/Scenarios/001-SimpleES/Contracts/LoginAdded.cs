using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Lokad.Cqrs.Scenarios.SimpleES.Contracts
{
    [DataContract]
    public sealed class LoginAdded : IAccountEvent
    {
        [DataMember]
        public readonly string Login;
        [DataMember]
        public readonly string Password;

        public LoginAdded(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}