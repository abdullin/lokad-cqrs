using Lokad.Cqrs.Directory;

namespace Lokad.Cqrs.Consume
{
	public sealed class MessageDirectoryCreated : ISystemEvent
	{
		public string Origin { get; private set; }
		public MessageInfo[] Messages { get; private set; }
		public ConsumerInfo[] Consumers { get; private set; }

		public MessageDirectoryCreated(string origin, MessageInfo[] messages, ConsumerInfo[] consumers)
		{
			Origin = origin;
			Messages = messages;
			Consumers = consumers;
		}
	}
}