using Lokad.Cqrs.Sample.Contracts;

namespace Lokad.Cqrs.Sample.Domain
{
    public sealed class InitializeSampleHandler : Define.Handle<InitializeSample>
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