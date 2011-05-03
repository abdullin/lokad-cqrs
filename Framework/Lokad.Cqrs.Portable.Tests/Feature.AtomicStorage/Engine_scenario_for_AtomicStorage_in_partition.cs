using Lokad.Cqrs.Build.Engine;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class Engine_scenario_for_AtomicStorage_in_partition : FiniteEngineScenario
    {

        public sealed class AtomicMessage : Define.Command { }

        public sealed class Entity
        {
            public int Count;
        }

        public sealed class Consumer : Define.Handle<AtomicMessage>
        {
            readonly IMessageSender _sender;
            readonly IAtomicSingletonWriter<Entity> _singleton;

            public Consumer(IMessageSender sender, IAtomicSingletonWriter<Entity> singleton)
            {
                _sender = sender;
                _singleton = singleton;
            }

            public void Consume(AtomicMessage atomicMessage, MessageContext context)
            {
                var entity = _singleton.AddOrUpdate(r => r.Count += 1);
                if (entity.Count == 5)
                {
                    _sender.SendOne(new AtomicMessage(), cb => cb.AddString("finish", ""));
                }
                else
                {
                    _sender.SendOne(new AtomicMessage());
                }
            }
        }
        public override void Configure(CloudEngineBuilder config)
        {
            config.Memory(m =>
            {
                m.AddMemoryProcess("do");
                m.AddMemorySender("do", cb => cb.IdGeneratorForTests());
            });

            StartupMessages.Add(new AtomicMessage());
        }
    }
}