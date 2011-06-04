using Farley.Engine.Design;
using FarleyFile;
using Lokad.Cqrs;

namespace Farley.Engine.Domain
{
    public sealed class InitializeSampleHandler : Handle<InitializeSample>
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
                    new AddTask("Read Lokad.CQRS Documentation"),
                    new AddTask("Start the sample")
                });
        }
    }
}