namespace Bus2.Build.Client
{
	public interface IClientBus
	{
		void SendMessage(object message);
	}
}