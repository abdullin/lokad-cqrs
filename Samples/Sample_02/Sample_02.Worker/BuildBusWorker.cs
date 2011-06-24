#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using Microsoft.WindowsAzure;

namespace Sample_02.Worker
{
    public static class BuildBusWorker
    {
        public static CqrsEngineBuilder Configure()
        {
            var builder = new CqrsEngineBuilder();
            
            builder.UseProtoBufSerialization();
            builder.Domain(d => d.HandlerSample<IConsume<IMessage>>(m => m.Consume(null)));

            var connection = AzureSettingsProvider.GetStringOrThrow("DiagnosticsConnectionString");
            var storageConfig = AzureStorage.CreateConfig(CloudStorageAccount.Parse(connection), c =>
            {
                c.ConfigureBlobClient(x => x.ReadAheadInBytes = 0x200000L);
                c.Named("dev");
            });

            builder.Azure(m =>
                {
                    m.AddAzureSender(storageConfig, "sample-02");

                    m.AddAzureProcess(storageConfig, "sample-02", x =>
                    {
                        x.DirectoryFilter(f => f.WhereMessagesAre<IMessage>());
                        x.DispatchAsEvents();
                    });
                });

            builder.Advanced.ConfigureContainer(WireTasks);

            return builder;
        }

        static void WireTasks(ContainerBuilder cb)
        {
            var happens = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => (a.FullName ?? "").Contains("Sample"))
                .SelectMany(t => t.GetExportedTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => (typeof(IEngineProcess)).IsAssignableFrom(t))
                .ToArray();

            Trace.WriteLine(string.Format("Discovered {0} tasks", happens.Length));
            foreach (var happen in happens)
            {
                cb.RegisterType(happen).As<IEngineProcess>();
            }
        }
    }
}
