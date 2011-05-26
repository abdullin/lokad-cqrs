using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryQueueWriterFactory :IQueueWriterFactory
    {
        readonly MemoryAccount _account;
        readonly string _endpoint;

        public MemoryQueueWriterFactory(MemoryAccount account, string endpoint = "memory")
        {
            _account = account;
            _endpoint = endpoint;
        }

        public string Endpoint
        {
            get { return _endpoint; }
        }

        public IQueueWriter GetWriteQueue(string queueName)
        {
            return
                new MemoryQueueWriter(
                    _account.Delivery.GetOrAdd(queueName, s => new BlockingCollection<ImmutableEnvelope>()), queueName);
        }
    }
}