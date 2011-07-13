using System;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Scenarios.SimpleES.Contracts;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    public sealed class AccountHandler : 
        Definitions.Define.Consumer<CreateAccount>,Definitions.Define.Consumer<AddLogin>

    {
        readonly Func<Definitions.Define.MyContext> _context;
        readonly AccountAggregateRepository _repository;
        readonly NuclearStorage _storage;

        public AccountHandler(Func<Definitions.Define.MyContext> context, AccountAggregateRepository repository, NuclearStorage storage)
        {
            _context = context;
            _storage = storage;
            _repository = repository;
        }

        public void Consume(CreateAccount command)
        {
            _repository.Append(_context().AggregateId, aar => aar.CreateAccount(command.Name));
        }

        public void Consume(AddLogin command)
        {
            _storage.UpdateSingleton<LoginIndex>(li => li.AddOrThrow(command.Username));
            _repository.Append(_context().AggregateId, aar => aar.AddLogin(command.Username,command.Password));
        }
    }
}