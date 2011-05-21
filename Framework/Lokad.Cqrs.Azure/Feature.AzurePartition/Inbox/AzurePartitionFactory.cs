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
        readonly IAzureStorageConfiguration _configuration;
        readonly TimeSpan _queueVisibilityTimeout;
        readonly AzureSchedulingProcess _process;
        readonly Func<uint, TimeSpan> _decayPolicy;

        readonly ConcurrentDictionary<string, AzurePartitionInboxIntake> _intakes = new ConcurrentDictionary<string, AzurePartitionInboxIntake>();

        public AzurePartitionFactory(
            IEnvelopeStreamer streamer, 
            ISystemObserver observer,
            IAzureStorageConfiguration configuration, 
            TimeSpan queueVisibilityTimeout,
            AzureSchedulingProcess process, 
            Func<uint, TimeSpan> decayPolicy)
        {
            _streamer = streamer;
            _queueVisibilityTimeout = queueVisibilityTimeout;
            _process = process;
            _decayPolicy = decayPolicy;
            _configuration = configuration;
            _observer = observer;
        }

        AzurePartitionInboxIntake BuildIntake(string name)
        {
            var queue = _configuration.CreateQueueClient().GetQueueReference(name);
            var container = _configuration.CreateBlobClient().GetContainerReference(name);
            
            var poisonQueue = new Lazy<CloudQueue>(() =>
                {
                    var queueReference = _configuration.CreateQueueClient().GetQueueReference(name + "-poison");
                    queueReference.CreateIfNotExist();
                    return queueReference;
                }, LazyThreadSafetyMode.ExecutionAndPublication);

            var reader = new StatelessAzureQueueReader(name, queue, container, poisonQueue,  _observer, _streamer, _queueVisibilityTimeout);
            var future = new StatelessAzureFutureList(container, _streamer);
            var writer = new StatelessAzureQueueWriter(_streamer, container, queue);

            return new AzurePartitionInboxIntake(writer, reader, future);
        }

        public IPartitionInbox GetNotifier(string[] queueNames)
        {

            var intakes = queueNames
                .Select(name => _intakes.GetOrAdd(name, BuildIntake))
                .ToArray();

            var writers = intakes.Select(i => i.Writer).ToArray();
            var futures = intakes.Select(i => i.Future).ToArray();
            var readers = intakes.Select(i => i.Reader).ToArray();

            var scheduler = new StatelessAzureScheduler(writers, futures);
            _process.AddScheduler(scheduler);

            
            return new AzurePartitionInbox(readers, futures, _decayPolicy);
        }
    }

    public sealed class AzurePartitionInboxIntake
    {
        public readonly StatelessAzureQueueWriter Writer;
        public readonly StatelessAzureQueueReader Reader;
        public readonly StatelessAzureFutureList Future;

        public AzurePartitionInboxIntake(StatelessAzureQueueWriter writer, StatelessAzureQueueReader reader, StatelessAzureFutureList future)
        {
            Future = future;
            Writer = writer;
            Reader = reader;
        }
    }
}