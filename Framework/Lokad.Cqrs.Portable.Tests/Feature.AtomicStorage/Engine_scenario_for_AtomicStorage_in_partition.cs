#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class Engine_scenario_for_AtomicStorage_in_partition : FiniteEngineScenario
    {
        [DataContract]
        public sealed class AtomicMessage : Define.Command {}

        [DataContract]
        public sealed class Entity : Define.AtomicEntity
        {
            [DataMember(Order = 1)]
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
                    _sender.SendOne(new AtomicMessage(), cb => cb.AddString("finish"));
                }
                else
                {
                    _sender.SendOne(new AtomicMessage());
                }
            }
        }

        protected override void Configure(CloudEngineBuilder config)
        {
            StartupMessages.Add(new AtomicMessage());
        }
    }
}