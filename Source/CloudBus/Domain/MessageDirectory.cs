namespace Bus2.Domain
{
	public sealed class MessageDirectory : IMessageDirectory
	{
		readonly string _consumeMethodName;
		readonly ConsumerInfo[] _consumers;
		readonly MessageInfo[] _messages;

		public MessageDirectory(string consumeMethodName, ConsumerInfo[] consumers, MessageInfo[] messages)
		{
			_consumeMethodName = consumeMethodName;
			_consumers = consumers;
			_messages = messages;
		}


		public ConsumerInfo[] Consumers
		{
			get { return _consumers; }
		}

		public MessageInfo[] Messages
		{
			get { return _messages; }
		}

		public void InvokeConsume(object consumer, object message)
		{
			MessageReflectionUtil.InvokeConsume(consumer, message, _consumeMethodName);
		}
	}
}