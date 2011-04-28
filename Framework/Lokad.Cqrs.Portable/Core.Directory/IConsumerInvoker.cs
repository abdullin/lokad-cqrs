namespace Lokad.Cqrs.Core.Directory
{
    public interface IConsumerInvoker
    {
        void InvokeConsume(object consumer, MessageItem item, MessageEnvelope envelope);
    }
}