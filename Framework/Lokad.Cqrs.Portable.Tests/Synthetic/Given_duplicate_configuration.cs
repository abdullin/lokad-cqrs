using System.Threading;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Core.Reactive;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_duplicate_configuration
    {
        // ReSharper disable InconsistentNaming


        public sealed class Message
        {
            
        }

        [Test]
        public void Dulicate_message_is_detected()
        {
            var builder = new CqrsEngineBuilder();
            builder.Memory(m =>
                {
                    m.AddMemorySender("in", s => s.IdGenerator(() => "same"));
                    m.AddMemoryRouter("in", c => "memory:null");
                });
            var observer = new ImmediateEventsObserver();
            builder.Observer(observer);

            using (var token = new CancellationTokenSource())
            using (var build = builder.Build())
            {
                var sender = build.Resolve<IMessageSender>();
                sender.SendOne(new Message());
                sender.SendOne(new Message());

                observer.Event += @event =>
                    {
                        var e = @event as EnvelopeDuplicateDiscarded;

                        if (e != null)
                        {
                            token.Cancel();
                        }
                    };
                build.Start(token.Token);
                token.Token.WaitHandle.WaitOne(10000);
                Assert.IsTrue(token.IsCancellationRequested);
            }
        }
    }
}