#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Directory.Default;
using NHibernate;
using NHibernate.Linq;

namespace Sample_03.Worker
{
    public sealed class CreateAccountFromTimeToTime : IEngineProcess
    {
        readonly ISessionFactory _sessionFactory;
        readonly IMessageSender _client;

        public CreateAccountFromTimeToTime(ISessionFactory sessionFactory, IMessageSender client)
        {
            _sessionFactory = sessionFactory;
            _client = client;
        }

        private TimeSpan ExecuteTask()
        {
            using (var session = _sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                // create new account
                var account = new AccountEntity();
                session.Save(account);

                // add initial balance of 0 to account
                var balanceEntity = new BalanceEntity
                    {
                        Change = 0,
                        Total = 0,
                        Name = "New balance",
                        Account = account
                    };

                session.Save(balanceEntity);

                Trace.WriteLine("Created account " + account.Id.ToReadable());
                _client.SendOne(new AddSomeBonusMessage(account.Id));

                tx.Commit();

                // sleep till the next run
                return TimeSpan.FromSeconds(10);
            }
        }

        public void Dispose()
        {}

        public void Initialize()
        {}

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var wait = ExecuteTask();
                        if (wait == TimeSpan.MaxValue)
                        {
                            // quit task
                            return;
                        }
                        token.WaitHandle.WaitOne(wait);

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                    }
                }
            });
        }
    }

    [DataContract]
    public sealed class AddSomeBonusMessage : IMessage
    {
        [DataMember]
        public Guid AccountId { get; private set; }

        public AddSomeBonusMessage(Guid accountId)
        {
            AccountId = accountId;
        }

        public override string ToString()
        {
            // helper for better trace logs
            return string.Format("AddSomeBonus to " + AccountId.ToReadable());
        }
    }

    public sealed class AddSomeBonusHandler : IConsume<AddSomeBonusMessage>
    {
        readonly ISession _session;
        readonly IMessageSender _client;

        public AddSomeBonusHandler(ISession session, IMessageSender client)
        {
            _session = session;
            _client = client;
        }

        public void Consume(AddSomeBonusMessage message)
        {
            // just to keep sample trace logs readable
            // in a nice way
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // we are using LINQ for NHibernate here
            var balance = _session
                .Linq<BalanceEntity>()
                .Where(e => e.Account.Id == message.AccountId)
                .OrderByDescending(e => e.Id)
                .Take(1)
                .FirstOrDefault();

            if (balance == null)
            {
                // we've got message in queue from the previous run of 
                // this sample app. ignore it,
                Trace.WriteLine("Account no longer exists. Ignore it");
                return;
            }

            if (balance.Total > 50)
            {
                Trace.WriteLine(string.Format(
                    "ENOUGH. Account {0} has too much money: {1}",
                    message.AccountId.ToReadable(),
                    balance.Total));
                return;
            }

            var total = balance.Total + 10;
            var bonus = new BalanceEntity
                {
                    Account = balance.Account,
                    Change = 10,
                    Name = "Bonus",
                    Total = total
                };

            _session.Save(bonus);

            Trace.WriteLine(string.Format(
                "Account {0} - adding {1} bonus with new total {2}",
                message.AccountId.ToReadable(), 10, total));
            _client.SendOne(new AddSomeBonusMessage(message.AccountId));

            // here's the interesting part.
            // If we throw exception here, then:
            // Nothing will be changed in the database
            // and messages will NOT be sent (volatile transactions).

            // this applies to the entire code block

            // after the message is processed, NHibernate transaction will
            // be completed and session - flushed
            // same with the message transactions
        }
    }
}