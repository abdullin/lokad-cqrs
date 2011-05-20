#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
    public sealed class AzureWriteQueueFactory : IQueueWriterFactory
    {
        readonly AzureAccessRegistry _configurations;
        readonly IEnvelopeStreamer _streamer;

        readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues =
            new ConcurrentDictionary<string, IQueueWriter>();

        public AzureWriteQueueFactory(
            AzureAccessRegistry accounts,
            IEnvelopeStreamer streamer)
        {
            _configurations = accounts;
            _streamer = streamer;
        }


        public bool TryGetWriteQueue(string endpointName, string queueName, out IQueueWriter writer)
        {
            IAzureAccessConfiguration config;
            if (_configurations.TryGet(endpointName, out config))
            {
                writer = _writeQueues.GetOrAdd(queueName, name =>
                    {
                        var queue = config.CreateQueueClient().GetQueueReference(name);
                        var container = config.CreateBlobClient().GetContainerReference(name);
                        var v = new StatelessAzureQueueWriter(_streamer, container, queue);
                        v.Init();
                        return v;
                    });
                return true;
            }

            writer = null;
            return false;
        }
    }
}