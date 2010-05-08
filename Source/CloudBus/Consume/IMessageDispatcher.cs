namespace Bus2.Consume
{
	public interface IMessageDispatcher
	{
		void Init();
		bool DispatchMessage(string topic, object message);
	}
}