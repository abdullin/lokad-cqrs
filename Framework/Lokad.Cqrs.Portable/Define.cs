using Lokad.Cqrs.Core.Directory.Default;

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

        public interface Consumer<TEvent> : IConsume<TEvent> where TEvent : Event
        {
            
        }

        public interface Handler<TCommand> : IConsume<TCommand> where TCommand : Command
        {
            
        }
    }
}