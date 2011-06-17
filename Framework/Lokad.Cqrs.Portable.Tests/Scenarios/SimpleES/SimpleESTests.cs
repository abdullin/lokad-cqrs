using System.Threading;
using Autofac;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Scenarios.SimpleES.Contracts;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;
using NUnit.Framework;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    [TestFixture, Explicit]
    public sealed class SimpleESTests
    {
        // ReSharper disable InconsistentNaming

        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();
            builder.Domain(m =>
                {
                    m.HandlerSample<Definitions.Define.Consumer<Definitions.Define.ICommand>>(c => c.Consume(null));
                    m.ContextFactory((e,x) => new Definitions.Define.MyContext(e.GetAttribute("EntityId","")));
                });
            builder.Storage(m => m.AtomicIsInMemory());
            builder.Memory(m =>
                {
                    m.AddMemorySender("in", c => c.IdGeneratorForTests());
                    m.AddMemoryProcess("in");
                });

            builder.Advanced.ConfigureContainer(cb =>
                {
                    cb.Register(c => new InMemoryEventStreamer<IAccountEvent>(c.Resolve<IMessageSender>())).
                        SingleInstance();

                    cb.RegisterType<InMemoryEventStreamer<IAccountEvent>>().SingleInstance();
                    cb.RegisterType<AccountAggregateRepository>().SingleInstance();

                });

            using (var source = new CancellationTokenSource())
            using (var engine = builder.Build())
            {
                engine.Start(source.Token);

                var sender = engine.Resolve<IMessageSender>();
                sender.SendOne(new CreateAccount("Sample User"), cb => cb.AddString("EntityId","1"));
                sender.SendOne(new AddLogin("login","pass"), cb => cb.AddString("EntityId", "1"));

                source.Token.WaitHandle.WaitOne(5000);
                source.Cancel();
            }
            
        }
        
    }
}