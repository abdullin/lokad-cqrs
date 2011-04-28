using Lokad.Cqrs.Core.Evil;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class DirectConsumer : IConsumerInvoker
    {
        readonly string _method;

        public DirectConsumer(string method)
        {
            _method = method;
        }

        public void InvokeConsume(object consumer, MessageItem item, MessageEnvelope envelope)
        {
            InvocationUtil.InvokeConsume(consumer, item.Content, _method);
        }
    }
}