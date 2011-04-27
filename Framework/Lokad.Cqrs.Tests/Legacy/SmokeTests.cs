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

namespace Lokad.Cqrs.Tests
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
                    x.AddPartition("azure:process-vip");
                    x.AddAccount("azure", CloudStorageAccount.DevelopmentStorageAccount);
                });
            engine.Memory(x =>
                {
                    
                    x.AddPartition("memory:process-all");
                    x.AddRouter("memory:inbox", e =>
                        {
                            if (e.Items.Any(i => i.MappedType == typeof (VipMessage)))
                                return "azure:process-vip";
                            return "memory:process-all";
                        });
                });


            engine.AddMessageClient("memory:inbox");

            engine.Memory(m =>
                {
                    m.AddRouter("inbox", )
                        .AddPartition("process-all");
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
            void Print(string value)
            {
                if (value.Length > 20)
                {
                    Trace.WriteLine(string.Format("{0}... ({1})", value.Substring(0, 16), value.Length));
                }
                else
                {
                    Trace.WriteLine(string.Format("{0}", value));
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

        [Test]
        public void Test2()
        {
            using (var host = BuildHost())
            using (var cts = new CancellationTokenSource())
            {
                host.Start(cts.Token);
                Console.WriteLine("Press any key to stop");
                Console.ReadKey(true);
                cts.Cancel(true);
            }
        }
    }
}