using System;
using Autofac;
using Lokad.Cqrs;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
using Lokad.Cqrs.Core.Outbox;
using Microsoft.WindowsAzure;

namespace Sample_01.Worker
{
    public static class BuildBusWorker
    {
        public static CqrsEngineBuilder Configure()
        {
            // for more detail about this sample see:
            // http://code.google.com/p/lokad-cqrs/wiki/GuidanceSeries

            var builder = new CqrsEngineBuilder();
            
            builder.UseProtoBufSerialization();
            builder.Domain(d => d.HandlerSample<IConsume<IMessage>>(m => m.Consume(null)));

            // TODO
            var connection = AzureSettingsProvider.GetString("StorageConnectionString");
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
                        x.DispatchAsCommandBatch();
                    });
                });

            return builder;
        }
    }
}
