#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class SmokeTests
    {
        // ReSharper disable InconsistentNaming

        #region Setup/Teardown

        static CqrsEngineHost BuildHost()
        {
            var builder = new CqrsEngineBuilder();
            builder.Azure(x =>
                {
                    x.AddAzureAccount("dev-store", CloudStorageAccount.DevelopmentStorageAccount);
                    x.AddAzureProcess("dev-store", "process-vip");
                });
            builder.Memory(x =>
                {
                    x.AddMemoryProcess("process-all");
                    x.AddMemoryRouter("inbox", e =>
                        {
                            var isVip = e.Items.Any(i => i.MappedType == typeof (VipMessage));
                            return isVip ? "dev-store:process-vip" : "memory:process-all";
                        });
                    x.AddMemorySender("inbox", cm => cm.IdGeneratorForTests());
                });


            return builder.Build();
        }

        #endregion

        [DataContract]
        public sealed class VipMessage : IMessage
        {
            [DataMember(Order = 1)]
            public string Word { get; set; }
        }

        [DataContract]
        public sealed class UsualMessage : IMessage
        {
            [DataMember(Order = 1)]
            public string Word { get; set; }
        }

        public sealed class DoSomething : IConsume<VipMessage>, IConsume<UsualMessage>
        {
            Func<MessageContext> _context;

            public DoSomething(Func<MessageContext> context)
            {
                _context = context;
            }

            void Print(string value)
            {
                if (value.Length > 20)
                {
                    Trace.WriteLine(string.Format("[{0}]: {1}... ({2})", _context().EnvelopeId, value.Substring(0, 16), value.Length));
                }
                else
                {
                    Trace.WriteLine(string.Format("[{0}]: {1}", value, _context().EnvelopeId));
                }
            }

            public void Consume(UsualMessage message)
            {
                Print(message.Word);
            }

            public void Consume(VipMessage message)
            {
                Print(message.Word);
            }
        }

        [Test]
        public void Test()
        {
            using (var host = BuildHost())
            {
                var client = host.Resolve<IMessageSender>();

                client.SendOne(new VipMessage {Word = "VIP1 Message"});
                client.SendOne(new UsualMessage {Word = "Usual Large:" + new string(')', 9000)});
                client.SendOne(new VipMessage {Word = "VIP Delayed Large :" + new string(')', 9000)}, cb => cb.DelayBy(TimeSpan.FromSeconds(3)));
                client.SendOne(new UsualMessage {Word = "Usual Delayed"}, cb => cb.DelayBy(TimeSpan.FromSeconds(2)));

                //client.SendBatch(new VipMessage { Word = " VIP with usual "}, new UsualMessage() { Word = "Vip with usual"});

                using (var cts = new CancellationTokenSource())
                {
                    var task = host.Start(cts.Token);
                    Thread.Sleep(TimeSpan.FromSeconds(10));


                    cts.Cancel(true);
                    task.Wait(TimeSpan.FromSeconds(5));
                }
                // second run
                using (var cts = new CancellationTokenSource())
                {
                    var task = host.Start(cts.Token);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel(true);
                    task.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}