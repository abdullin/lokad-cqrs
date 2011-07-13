using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;
using System.Linq;

namespace Lokad.Cqrs.Scenarios.Multiverse
{
    [TestFixture]
    public sealed class MultiverseTests
    {
        // ReSharper disable InconsistentNaming

        public interface IConsume<in T>
        {
            void Consume(T message);
        }

        public interface IBaseMessage1 {}

        public interface IBaseMessage2 {}

        [DataContract]
        public sealed class Message1 :IBaseMessage1{}
        [DataContract]
        public sealed class Message2 : IBaseMessage2{}

        public sealed class MultiVerseHandler : 
            IConsume<Message1>, IConsume<Message2>
        {
            public void Consume(Message1 message)
            {
                
            }

            public void Consume(Message2 message)
            {
                
            }
        }

        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();
            builder.Domain(mdm =>
                {
                    mdm.HandlerSample<IConsume<object>>(c => c.Consume(null));
                    mdm.WhereMessages(t => typeof(IBaseMessage1).IsAssignableFrom(t) || typeof(IBaseMessage2).IsAssignableFrom(t));
                    mdm.InAssemblyOf(this);
                });

            builder.Memory(m =>
                {
                    m.AddMemoryProcess("in");
                    m.AddMemorySender("in");
                });

            var subj = new Subject<ISystemEvent>();
            builder.Advanced.Observers.Clear();
            builder.Advanced.Observers.Add(subj);


            
            using (var engine = builder.Build())
            using (var source = new CancellationTokenSource())
            {
                int counter = 0;
                subj.Where(t => t is EnvelopeAcked).Subscribe(e =>
                    {
                        if (++counter == 2)
                            source.Cancel();
                    });

                engine.Start(source.Token);
                var sender = engine.Resolve<IMessageSender>();
                sender.SendOne(new Message1());
                sender.SendOne(new Message2());

                if (!source.Token.WaitHandle.WaitOne(3000))
                {
                    Assert.Fail("This should've been canceled by now");
                }
            }
        }
         
    }
}