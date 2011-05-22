#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
    public sealed class AzureQueueWriterFactory : IQueueWriterFactory
    {
        readonly IAzureStorageConfig _config;
        readonly IEnvelopeStreamer _streamer;

        readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues =
            new ConcurrentDictionary<string, IQueueWriter>();

        public AzureQueueWriterFactory(
            IAzureStorageConfig accounts,
            IEnvelopeStreamer streamer)
        {
            _config = accounts;
            _streamer = streamer;
        }

        public string Endpoint { get { return _config.AccountName; } }


        public IQueueWriter GetWriteQueue(string queueName)
        {
            return _writeQueues.GetOrAdd(queueName, name =>
            {
                var queue = _config.CreateQueueClient().GetQueueReference(name);
                var container = _config.CreateBlobClient().GetContainerReference(name);
                var v = new StatelessAzureQueueWriter(_streamer, container, queue, name);
                v.Init();
                return v;
            });
        }
    }
}