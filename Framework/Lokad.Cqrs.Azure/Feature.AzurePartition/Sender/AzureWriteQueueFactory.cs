#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
    public sealed class AzureWriteQueueFactory : IQueueWriterFactory
    {
        readonly IAzureStorageConfiguration[] _configurations;
        readonly IEnvelopeStreamer _streamer;

        readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues =
            new ConcurrentDictionary<string, IQueueWriter>();

        public AzureWriteQueueFactory(
            IEnumerable<IAzureStorageConfiguration> accounts,
            IEnvelopeStreamer streamer)
        {
            _configurations = accounts.ToArray();
            _streamer = streamer;
        }


        public bool TryGetWriteQueue(string endpointName, string queueName, out IQueueWriter writer)
        {
            foreach (var configuration in _configurations)
            {
                if (endpointName != configuration.AccountName) continue;

                writer = _writeQueues.GetOrAdd(queueName, name =>
                    {
                        var queue = configuration.CreateQueueClient().GetQueueReference(name);
                        var container = configuration.CreateBlobClient().GetContainerReference(name);
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