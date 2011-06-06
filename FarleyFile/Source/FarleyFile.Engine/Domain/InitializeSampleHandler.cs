using Lokad.Cqrs;

namespace FarleyFile.Engine.Domain
{
    public sealed class InitializeSampleHandler : Farley.Handle<InitializeSample>
    {
        readonly IMessageSender _sender;

        public InitializeSampleHandler(IMessageSender sender)
        {
            _sender = sender;
        }

        public void Consume(InitializeSample message)
        {
            _sender.SendBatch(new object[]
                {
                    new AddContact("Read Lokad.CQRS Documentation"),
                    new AddContact("Start the sample")
                });
        }
    }
}