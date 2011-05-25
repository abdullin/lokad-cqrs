using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Lokad.Cqrs;
using Lokad.Cqrs.Build;
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

            // TODO
            var connection = AzureSettingsProvider.GetString("DiagnosticsConnectionString");
            var storageConfig = AzureStorage.CreateConfig(CloudStorageAccount.Parse(connection), c =>
            {
                c.ConfigureBlobClient(x => x.ReadAheadInBytes = 0x200000L);
                c.Named("dev");
            });

            builder.Azure(m =>
                {
                    m.AddAzureSender(storageConfig, NameFor.Queue);
 
                    m.AddAzureProcess(storageConfig, NameFor.Queue, x =>
                    {
                        x.DirectoryFilter(f => f.WhereMessagesAre<IMessage>());
                        x.DispatchAsEvents();
                    });
                });

            builder.Advanced(cb =>
                {
                    WireTasks(cb);
                });

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
