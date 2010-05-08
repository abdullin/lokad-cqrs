using System.Reflection;

namespace Bus2.Domain
{
	public interface IMessageDirectory
	{
		ConsumerInfo[] Consumers { get; }
		MessageInfo[] Messages { get; }
		void InvokeConsume(object consumer, object message);
	}
}