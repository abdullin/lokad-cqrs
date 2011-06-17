namespace Lokad.Cqrs.Scenarios.SimpleES.Definitions
{
    public static class Define
    {
        public interface ICommand
        {
            
        }

        public interface Consumer<T> where T : ICommand
        {
            void Consume(T command);
        }

        public sealed class MyContext
        {
            public readonly string AggregateId;

            public MyContext(string aggregateId)
            {
                AggregateId = aggregateId;
            }
        }
    }
}