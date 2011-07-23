#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory.Default;
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

            var connection = AzureSettingsProvider.GetString("StorageConnectionString");
            var storageConfig = AzureStorage.CreateConfig(CloudStorageAccount.Parse(connection), c =>
            {
                c.ConfigureBlobClient(x => x.ReadAheadInBytes = 0x200000L);
                c.Named("dev");
            });

            builder.Azure(m =>
                {
                    m.AddAzureSender(storageConfig, "sample - 01");
 
                    m.AddAzureProcess(storageConfig, "sample-01", x =>
                    {
                        x.DirectoryFilter(f => f.WhereMessagesAre<IMessage>());
                        x.DispatchAsCommandBatch();
                    });
                });

            return builder;
        }
    }
}
