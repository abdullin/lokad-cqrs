using System;
using System.Collections.Generic;
using Lokad.Cqrs.Build.Engine;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public abstract class IFiniteEngineScenario
    {
        public bool HandlerFailuresAreExpected;
        public readonly IList<object> StartupMessages = new List<object>();

        
        public abstract void Configure(CloudEngineBuilder config);


    }
    public sealed class Engine_scenario_for_NuclearStorage_in_partition : IFiniteEngineScenario
    {
        public sealed class Entity
        {
            public int Count;
        }

        public sealed class NuclearMessage : Define.Command { }

        public sealed class NuclearHandler : Define.Handle<NuclearMessage>
        {
            readonly IMessageSender _sender;
            readonly NuclearStorage _storage;

            public NuclearHandler(IMessageSender sender, NuclearStorage storage)
            {
                _sender = sender;
                _storage = storage;
            }

            public void Consume(NuclearMessage atomicMessage, MessageContext context)
            {
                var result = _storage
                    .AddOrUpdateSingleton<Entity>(s => s.Count += 1);

                if (result.Count == 5)
                {
                    _sender.SendOne(new NuclearMessage(), cb => cb.AddString("finish", ""));
                }
                else
                {
                    _sender.SendOne(new NuclearMessage());
                }
            }
        }


        public override void Configure(CloudEngineBuilder config)
        {
            config.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemorySender("do");
                });

            StartupMessages.Add(new NuclearMessage());
        }
    }
}