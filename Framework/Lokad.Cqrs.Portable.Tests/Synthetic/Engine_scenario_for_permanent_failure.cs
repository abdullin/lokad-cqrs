#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    public sealed class Engine_scenario_for_permanent_failure : FiniteEngineScenario
    {
        [DataContract]
        public sealed class Message : Define.Command {}

        public sealed class Handler : Define.Handle<Message>
        {
            public void Consume(Message atomicMessage, MessageContext context)
            {
                throw new NotImplementedException("Fail: try");
            }
        }


        protected override void Configure(CloudEngineBuilder builder)
        {
            HandlerFailuresAreExpected = true;

            Enlist((observable, sender, arg3) => observable
                .OfType<EnvelopeQuarantined>()
                .Subscribe(eq =>
                    {
                        Assert.AreEqual("Fail: try", eq.LastException.Message);
                        arg3.Cancel();
                    }));

            StartupMessages.Add(new Message());
        }
    }
}