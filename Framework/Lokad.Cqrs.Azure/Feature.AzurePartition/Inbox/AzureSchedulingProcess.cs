using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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
}