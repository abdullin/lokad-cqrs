#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace Lokad.Cqrs.Legacy
{
    [TestFixture]
    public sealed class SmokeTests
    {
        // ReSharper disable InconsistentNaming

        #region Setup/Teardown

        static CloudEngineHost BuildHost()
        {
            var engine = new CloudEngineBuilder();
            engine.Azure(x =>
                {
                    x.AddAzureAccount("dev-store", CloudStorageAccount.DevelopmentStorageAccount);
                    x.AddAzureProcess("dev-store", "process-vip");
                });
            engine.Memory(x =>
                {
                    x.AddMemoryProcess("process-all");
                    x.AddMemoryRouter("inbox", e =>
                        {
                            var isVip = e.Items.Any(i => i.MappedType == typeof (VipMessage));
                            return isVip ? "dev-store:process-vip" : "memory:process-all";
                        });
                    x.AddMemorySender("inbox", cm => cm.IdGeneratorForTests());
                });


            return engine.Build();
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
            void Print(string value, MessageDetail detail)
            {
                if (value.Length > 20)
                {
                    Trace.WriteLine(string.Format("[{0}]: {1}... ({2})", detail.EnvelopeId, value.Substring(0, 16), value.Length));
                }
                else
                {
                    Trace.WriteLine(string.Format("[{0}]: {1}", value, detail.EnvelopeId));
                }
            }

            public void Consume(UsualMessage message, MessageDetail detail)
            {
                Print(message.Word, detail);
            }

            public void Consume(VipMessage message, MessageDetail detail)
            {
                Print(message.Word, detail);
            }
        }

        [Test]
        public void Test()
        {
            using (var host = BuildHost())
            {
                var client = host.Resolve<IMessageSender>();

                client.Send(new VipMessage {Word = "VIP1 Message"});
                client.Send(new UsualMessage {Word = "Usual Large:" + new string(')', 9000)});
                client.DelaySend(3.Seconds(), new VipMessage {Word = "VIP Delayed Large :" + new string(')', 9000)});
                client.DelaySend(2.Seconds(), new UsualMessage {Word = "Usual Delayed"});

                //client.SendBatch(new VipMessage { Word = " VIP with usual "}, new UsualMessage() { Word = "Vip with usual"});

                using (var cts = new CancellationTokenSource())
                {
                    var task = host.Start(cts.Token);
                    Thread.Sleep(10.Seconds());


                    cts.Cancel(true);
                    task.Wait(5.Seconds());
                }
                // second run
                using (var cts = new CancellationTokenSource())
                {
                    var task = host.Start(cts.Token);
                    Thread.Sleep(2.Seconds());
                    cts.Cancel(true);
                    task.Wait(5.Seconds());
                }
            }
        }
    }
}