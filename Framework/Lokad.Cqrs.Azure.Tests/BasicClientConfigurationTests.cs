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
            var dev = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), dev);

            var events = new Subject<ISystemEvent>();
            var b = new CqrsEngineBuilder();
            b.Azure(c => c.AddAzureProcess(dev, "test-publish"));
            b.Advanced.RegisterObserver(events);
            var engine = b.Build();
            var source = new CancellationTokenSource();
            engine.Start(source.Token);




            var builder = new CqrsClientBuilder();
            builder.Azure(c => c.AddAzureSender(dev, "test-publish"));
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