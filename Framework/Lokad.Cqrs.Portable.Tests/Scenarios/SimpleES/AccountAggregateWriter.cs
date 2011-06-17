using System;
using Lokad.Cqrs.Scenarios.SimpleES.Contracts;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    public sealed class AccountAggregateWriter : AccountAggregateReader
    {
        public void CreateAccount(string name)
        {
            Apply(new AccountCreated(name));
        }

        public void AddLogin(string login, string password)
        {
            Apply(new LoginAdded(login, password));
        }

        public void Apply(IAccountEvent e)
        {
            RedirectToWhen<AccountAggregateReader>.Invoke(this, e);
            _observer.OnNext(e);
        }


        readonly IObserver<IAccountEvent> _observer;

        public AccountAggregateWriter(IObserver<IAccountEvent> observer)
        {
            _observer = observer;
        }
    }
}