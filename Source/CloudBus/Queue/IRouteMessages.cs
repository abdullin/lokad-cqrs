namespace Bus2.Queue
{
	public interface IRouteMessages
	{
		void RouteMessages(IncomingMessage[] message, params string[] references);
	}
}