using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryQueueWriterFactory :IQueueWriterFactory
    {
        readonly MemoryAccount _account;

        public MemoryQueueWriterFactory(MemoryAccount account)
        {
            _account = account;
        }

        public string Endpoint
        {
            get { return "memory"; }
        }

        public IQueueWriter GetWriteQueue(string queueName)
        {
            return
                new MemoryQueueWriter(
                    _account.Delivery.GetOrAdd(queueName, s => new BlockingCollection<ImmutableEnvelope>()), queueName);
        }
    }
}