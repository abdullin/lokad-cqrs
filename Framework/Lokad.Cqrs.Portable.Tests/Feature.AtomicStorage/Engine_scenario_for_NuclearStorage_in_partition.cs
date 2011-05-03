using System;
using Lokad.Cqrs.Build.Engine;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IFiniteEngineScenario
    {
        Define.Command Start();
        void Configure(CloudEngineBuilder config);
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

        public Define.Command Start()
        {
            return new NuclearMessage();
        }

        public void Configure(CloudEngineBuilder config)
        {
            config.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemoryAtomicStorage();
                    m.AddMemorySender("do");
                });
        }
    }
}