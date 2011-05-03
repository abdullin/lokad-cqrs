#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class Engine_scenario_for_NuclearStorage_in_partition : FiniteEngineScenario
    {
        [DataContract]
        public sealed class Entity : Define.AtomicEntity
        {
            [DataMember(Order = 1)] public int Count;
        }

        [DataContract]
        public sealed class NuclearMessage : Define.Command {}

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


        protected override void Configure(CloudEngineBuilder builder)
        {
            StartupMessages.Add(new NuclearMessage());
        }
    }
}