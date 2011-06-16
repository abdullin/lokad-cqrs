using System;

namespace Lokad.Cqrs.Core.Dispatch
{
    /// <summary>
    /// Logical transaction and resolution hierarchy for dispatching this specific message.
    /// </summary>
    public interface IMessageDispatchScope : IDisposable
    {
        /// <summary>
        /// Dispatches the specified message to instance of the consumer type.
        /// </summary>
        /// <param name="consumerType">Type of the consumer expected.</param>
        /// <param name="envelope">The envelope context.</param>
        /// <param name="message">The actual message to dispatch.</param>
        void Dispatch(Type consumerType, ImmutableEnvelope envelope, ImmutableMessage message);
        /// <summary>
        /// Completes this scope.
        /// </summary>
        void Complete();
    }
}