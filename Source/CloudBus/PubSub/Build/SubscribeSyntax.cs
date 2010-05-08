namespace Bus2.PubSub.Build
{
	public sealed class SubscribeSyntax
	{
		readonly IPublishSubscribeManager _manager;

		public SubscribeSyntax(IPublishSubscribeManager manager)
		{
			_manager = manager;
		}

		public SubscribeSyntax DirectBinding(string topic, string queueName)
		{
			var id = string.Format("direct-{0}-{1}", topic, queueName);
			_manager.SubscribeDirect(id, topic, queueName);
			return this;
		}

		public SubscribeSyntax WithRegex(string regex, string queueName)
		{
			var id = string.Format("regex-{0}-{1}", regex, queueName);
			_manager.SubscribeRegex(id, regex, queueName);
			return this;
		}
	}
}