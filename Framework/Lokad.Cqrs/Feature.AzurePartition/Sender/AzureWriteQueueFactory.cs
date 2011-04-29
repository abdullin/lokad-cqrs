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
        readonly IAzureClientConfiguration[] _configurations;
        readonly IEnvelopeStreamer _streamer;

        readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues =
            new ConcurrentDictionary<string, IQueueWriter>();

        public AzureWriteQueueFactory(
            IEnumerable<IAzureClientConfiguration> accounts,
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
                        var queue = configuration.BuildQueue(name);
                        var container = configuration.BuildContainer(name);
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