#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
    public sealed class AzurePartitionFactory 
    {
        readonly IEnvelopeStreamer _streamer;
        readonly ISystemObserver _observer;
        readonly IAzureStorageConfig _config;
        readonly TimeSpan _queueVisibilityTimeout;
        readonly Func<uint, TimeSpan> _decayPolicy;

        readonly ConcurrentDictionary<string, StatelessAzureQueueReader> _intakes = new ConcurrentDictionary<string, StatelessAzureQueueReader>();

        public AzurePartitionFactory(
            IEnvelopeStreamer streamer, 
            ISystemObserver observer,
            IAzureStorageConfig config, 
            TimeSpan queueVisibilityTimeout,
            Func<uint, TimeSpan> decayPolicy)
        {
            _streamer = streamer;
            _queueVisibilityTimeout = queueVisibilityTimeout;
            _decayPolicy = decayPolicy;
            _config = config;
            _observer = observer;
        }

        StatelessAzureQueueReader BuildIntake(string name)
        {
            var queue = _config.CreateQueueClient().GetQueueReference(name);
            var container = _config.CreateBlobClient().GetContainerReference(name);
            
            var poisonQueue = new Lazy<CloudQueue>(() =>
                {
                    var queueReference = _config.CreateQueueClient().GetQueueReference(name + "-poison");
                    queueReference.CreateIfNotExist();
                    return queueReference;
                }, LazyThreadSafetyMode.ExecutionAndPublication);

            var reader = new StatelessAzureQueueReader(name, queue, container, poisonQueue,  _observer, _streamer, _queueVisibilityTimeout);

            return reader;
        }

        public IPartitionInbox GetNotifier(string[] queueNames)
        {

            var readers = queueNames
                .Select(name => _intakes.GetOrAdd(name, BuildIntake))
                .ToArray();
            
            return new AzurePartitionInbox(readers, _decayPolicy);
        }
    }
}