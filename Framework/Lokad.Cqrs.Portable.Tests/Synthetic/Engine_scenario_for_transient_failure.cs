#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.Synthetic
{
    public sealed class Engine_scenario_for_transient_failure : FiniteEngineScenario
    {
        [DataContract]
        public sealed class Message : Define.Command {}


        public sealed class Handler : Define.Handle<Message>
        {
            readonly NuclearStorage _storage;
            readonly IMessageSender _sender;

            public Handler(NuclearStorage storage, IMessageSender sender)
            {
                _storage = storage;
                _sender = sender;
            }

            public void Consume(Message message)
            {
                var count = _storage.AddOrUpdateSingleton(() => 1, i => i + 1);
                if (count < 4)
                    throw new InvalidOperationException("Fail");

                _sender.SendOne(new Message(), cb => cb.AddString("finish"));
            }
        }

        protected override void Configure(CqrsEngineBuilder b)
        {
            HandlerFailuresAreExpected = true;

            EnlistMessage(new Message());
        }
    }
}