namespace Lokad.Cqrs.Core.Directory
{
    public interface IMethodInvoker
    {
        void InvokeConsume(object consumer, ImmutableMessage item, ImmutableEnvelope envelope);
    }
}