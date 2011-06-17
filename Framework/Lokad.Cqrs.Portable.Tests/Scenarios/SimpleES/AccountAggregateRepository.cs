using System;
using System.Collections.Generic;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    public sealed class AccountAggregateRepository
    {
        readonly InMemoryEventStreamer<IAccountEvent> _streams;

        public AccountAggregateRepository(InMemoryEventStreamer<IAccountEvent> streams)
        {
            _streams = streams;
        }


        public void Append(string key, Action<AccountAggregateWriter> update)
        {
            _streams.Append(key, before =>
                {
                    var obs = new Subject<IAccountEvent>();
                    var ar = new AccountAggregateWriter(obs);
                    var list = new List<IAccountEvent>();
                    foreach (var @event in before)
                    {
                        ar.Apply(@event);
                    }
                    using (obs.Subscribe(list.Add))
                    {
                        update(ar);
                    }
                    return list;
                });
        }

    }
}