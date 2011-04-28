namespace Lokad.Cqrs.Core.Directory
{
    public interface IMethodInvoker
    {
        void InvokeConsume(object consumer, MessageItem item, MessageEnvelope envelope);
    }
}