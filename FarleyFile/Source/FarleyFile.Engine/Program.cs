#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using FarleyFile;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;

namespace Farley.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());


            var builder = new CqrsEngineBuilder();
            builder.Storage(m => m.AtomicIsInMemory());
            builder.Memory(m =>
                {
                    m.AddMemoryProcess("in");
                    m.AddMemorySender("in");
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