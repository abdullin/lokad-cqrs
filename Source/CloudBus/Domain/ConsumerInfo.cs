using System;

namespace Bus2.Domain
{
	public sealed class ConsumerInfo
	{
		public readonly Type ConsumerType;
		public readonly Type[] MessageTypes;

		public ConsumerInfo(Type consumerType, Type[] messageTypes)
		{
			ConsumerType = consumerType;
			MessageTypes = messageTypes;
		}
	}
}