#region Copyright (c) 2009-2011 LOKAD SAS. All rights reserved.

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Engine;
using ServiceStack.Text;


namespace FarleyFile.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var builder = new CqrsEngineBuilder();

            var config = AzureStorage.CreateConfigurationForDev();
            builder.Domain(d => d.HandlerSample<Farley.IFarleyHandler<Farley.IMessage>>(m => m.Consume(null)));

            
            builder.Storage(m => m.AtomicIsInAzure(config, b =>
                {
                    b.CustomSerializer(JsonSerializer.SerializeToStream, JsonSerializer.DeserializeFromStream);
                    b.WhereEntity(t => t.Name.EndsWith("View") && t.IsDefined(typeof(SerializableAttribute), false));
                }));
            builder.Azure(m =>
                {
                    m.AddAzureRouter(config, "farley-inbox", i => i.Items[0].Content is Farley.Command ? "farley-commands" : "farley-events");
                    m.AddAzureSender(config, "farley-inbox");
                    m.AddAzureProcess(config, "farley-commands", x => x.DirectoryFilter(f => f.WhereMessagesAre<Farley.Command>()));
                    m.AddAzureProcess(config, "farley-events", x => x.DirectoryFilter(f => f.WhereMessagesAre<Farley.Event>()));
                });

            
            try
            {
                using (var token = new CancellationTokenSource())
                using (var engine = builder.Build())
                {
                    engine.Start(token.Token);

                    engine
                        .Resolve<IMessageSender>()
                        .SendOne(new InitializeSample());


                    Console.WriteLine("Press any key to stop.");
                    Console.ReadKey(true);

                    token.Cancel();
                    if (!token.Token.WaitHandle.WaitOne(5000))
                    {
                        Console.WriteLine("Terminating");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey(true);
            }
        }
    }
}