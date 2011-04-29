#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Inbox;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
    public sealed class AzureSchedulingProcess : IEngineProcess
    {
        readonly ConcurrentBag<StatelessAzureScheduler> _schedulers = new ConcurrentBag<StatelessAzureScheduler>();

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var scheduler in _schedulers)
                    {
                        scheduler.DispatchDelayedMessages();
                    }

                    token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                }
            });
        }

        public void AddScheduler(StatelessAzureScheduler scheduler)
        {
            _schedulers.Add(scheduler);
        }
    }

    
    public sealed class AzurePartitionFactory 
    {
        readonly IEnvelopeStreamer _streamer;
        readonly ISystemObserver _observer;
        readonly IAzureClientConfiguration _configuration;

        
        readonly TimeSpan _queueVisibilityTimeout;
        readonly AzureSchedulingProcess _process;

        readonly ConcurrentDictionary<string, AzurePartitionInboxIntake> _intakes = new ConcurrentDictionary<string, AzurePartitionInboxIntake>();

        public AzurePartitionFactory(
            IEnvelopeStreamer streamer, 
            ISystemObserver observer,
            IAzureClientConfiguration configuration, 
            TimeSpan queueVisibilityTimeout,
            AzureSchedulingProcess process)
        {
            _streamer = streamer;
            _queueVisibilityTimeout = queueVisibilityTimeout;
            _process = process;
            _configuration = configuration;
            _observer = observer;
        }

        public void SetupForTesting()
        {
            foreach (var intake in _intakes.Values)
            {
                intake.SetupForTesting();
            }
        }

        AzurePartitionInboxIntake BuildIntake(string name)
        {
            var queue = _configuration.BuildQueue(name);
            var container = _configuration.BuildContainer(name);
            
            var poisonQueue = new Lazy<CloudQueue>(() =>
                {
                    var queueReference = _configuration.BuildQueue(name + "-poison");
                    queueReference.CreateIfNotExist();
                    return queueReference;
                }, LazyThreadSafetyMode.ExecutionAndPublication);

            var reader = new StatelessAzureQueueReader(name, queue, container, poisonQueue,  _observer, _streamer, _queueVisibilityTimeout);
            var future = new StatelessAzureFutureList(container, _streamer);
            var writer = new StatelessAzureQueueWriter(_streamer, container, queue);

            return new AzurePartitionInboxIntake(name, writer, reader, future);
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

            var decayPolicy = BuildDecayPolicy(TimeSpan.FromSeconds(2));
            return new AzurePartitionInbox(readers, futures, decayPolicy);
        }

        static Func<uint, TimeSpan> BuildDecayPolicy(TimeSpan maxDecay)
        {
            //var seconds = (Rand.Next(0, 1000) / 10000d).Seconds();
            var seconds = maxDecay.TotalSeconds;
            return l =>
                {
                    if (l >= 31)
                    {
                        return maxDecay;
                    }

                    if (l == 0)
                    {
                        l += 1;
                    }

                    var foo = Math.Pow(2, (l - 1)/5.0)/64d*seconds;

                    return TimeSpan.FromSeconds(foo);
                };
        }

   

        
    }

    public sealed class AzurePartitionInboxIntake
    {
        public readonly string Name;
        public readonly StatelessAzureQueueWriter Writer;
        public readonly StatelessAzureQueueReader Reader;
        public readonly StatelessAzureFutureList Future;

        public AzurePartitionInboxIntake(string name, StatelessAzureQueueWriter writer, StatelessAzureQueueReader reader, StatelessAzureFutureList future)
        {
            Name = name;
            Future = future;
            Writer = writer;
            Reader = reader;
        }

        public void SetupForTesting()
        {
            Reader.SetupForTesting();
            Future.SetupForTesting();
        }
    }
}