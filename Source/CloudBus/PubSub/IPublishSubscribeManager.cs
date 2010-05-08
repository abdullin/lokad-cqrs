namespace Bus2.PubSub
{
	/// <summary>
	/// Analogue of the AMQP pub/sub
	/// </summary>
	public interface IPublishSubscribeManager
	{
		string[] GetSubscribers(string topic);

		void SubscribeDirect(string subscriptionId, string topic, string reference);
		void SubscribeRegex(string subscriptionId, string regex, string reference);
		void Unsubscribe(string subscriptionId);
		//void SubscribeRegex(string regex, QueueReference reference);
	}
}