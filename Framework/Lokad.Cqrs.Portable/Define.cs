using Lokad.Cqrs.Build.Client;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.Dispatch.Directory.Default;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Default implementations of the domain-specific interfaces
    /// </summary>
    public static class Define
    {
        public interface Event : IMessage
        {

        }
        public interface Command : IMessage
        {

        }

        public interface Subscribe<TEvent> : IConsume<TEvent> where TEvent : Event
        {
            
        }

        public interface Handle<TCommand> : IConsume<TCommand> where TCommand : Command
        {
            
        }

        public interface AtomicEntity
        {
            
        }
        public interface AtomicSingleton
        {
            
        }
    }
}