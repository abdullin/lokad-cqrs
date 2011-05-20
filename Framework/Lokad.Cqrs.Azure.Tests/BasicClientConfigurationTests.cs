using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Client;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using Microsoft.WindowsAzure;
using NUnit.Framework;
using System.Linq;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class BasicClientConfigurationTests
    {
        [DataContract]
        public sealed class Message : Define.Command
        {
            
        }

        
        public sealed class Handler : Define.Handle<Message>
        {
            public void Consume(Message message)
            {
                
            }
        }



        // ReSharper disable InconsistentNaming
        [Test]
        public void Test()
        {
            var events = new Subject<ISystemEvent>();

            var eb = new CqrsEngineBuilder();
            eb.Azure(c =>
                {
                    c.AddAzureProcess("azure-dev", "publish");
                    c.WipeAccountsAtStartUp = true;
                });
            eb.EnlistObserver(events);
            var engine = eb.Build();
            var source = new CancellationTokenSource();
            engine.Start(source.Token);




            var builder = new CqrsClientBuilder();
            builder.Azure(c => c.AddAzureSender("azure-dev", "publish"));
            var client = builder.Build();


            client.Sender.SendOne(new Message());

            using (engine)
            using (events.OfType<EnvelopeAcked>().Subscribe(e => source.Cancel()))
            {
                source.Token.WaitHandle.WaitOne(5000);
                source.Cancel();
            }
        }
    }
}