using System;

namespace Lokad.Cqrs.Core.Dispatch
{
    public interface IMessageDispatchScope : IDisposable
    {
        void Dispatch(Type consumerType, ImmutableEnvelope envelop, ImmutableMessage message);
        void Complete();
    }
}