using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemorySchedulingProcess : IEngineProcess
    {
        readonly MemoryAccount _account;

        public MemorySchedulingProcess(MemoryAccount account)
        {
            _account = account;
        }

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
                        foreach (var list in _account.Pending)
                        {
                            ImmutableEnvelope envelope;
                            while (list.Value.TakePendingMessage(out envelope))
                            {
                                _account.Delivery
                                    .GetOrAdd(list.Key, n => new BlockingCollection<ImmutableEnvelope>())
                                    .Add(envelope);
                            }
                        }

                        token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    }
                });
        }
    }
}