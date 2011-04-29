namespace Lokad.Cqrs.Core.Directory
{
    public interface IMethodInvoker
    {
        void InvokeConsume(object consumer, ImmutableMessageItem item, ImmutableMessageEnvelope envelope);
    }
}