using System;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;

namespace Sample_01.Worker
{
    public class MessageDispatcher : ISingleThreadMessageDispatcher
    {
        IQueueWriter _factory;

        public MessageDispatcher(QueueWriterRegistry registry, string accountName)
        {
            IQueueWriterFactory factory;
            if (registry.TryGet(accountName, out factory))
            {
                _factory = factory.GetWriteQueue(NameFor.Queue);
                
            }

            var message = string.Format("Failed to load factory for route '{0}:{1}'", accountName, NameFor.Queue);
            throw new InvalidOperationException(message);
        }

        public void DispatchMessage(ImmutableEnvelope message)
        {
            _factory.PutMessage(message);
        }

        public void Init()
        {
        }
    }
}