using System;
using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Feature.AtomicStorage;
using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    public sealed class Engine_scenario_for_transactional_commands : FiniteEngineScenario
    {
        [DataContract]
        public sealed class Message : Define.Command
        {
            [DataMember]
            public bool Fail;
        }

        public sealed class Handler : Define.Handle<Message>
        {
            readonly NuclearStorage _storage;
            public Handler(NuclearStorage storage)
            {
                _storage = storage;
            }

            public void Consume(Message message, MessageContext context)
            {
                if (message.Fail)
                    throw new InvalidOperationException("too much in one transaction");


                new TransactionTester()
                    {
                        OnCommit = () => _storage.UpdateSingletonStruct<int>(i => i + 1)
                    };
            }
        }

        protected override void Configure(CqrsEngineBuilder builder)
        {
            HandlerFailuresAreExpected = true;

            EnlistMessage(new Message(), new Message());
            EnlistMessage(new Message(), new Message(), new Message { Fail = true });

            Enlist((observable, sender, arg3) => observable
                .OfType<EnvelopeDispatchFailed>()
                .Subscribe(e => arg3.Cancel()));

            EnlistAssert(e =>
                {
                    var actual = e.Resolve<NuclearStorage>().GetSingletonOrDefault<int>();
                    Assert.AreEqual(2, actual, "Only first command batch should succeed");
                });
        }
    }
}