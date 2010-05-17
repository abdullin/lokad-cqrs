#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace CloudBus.PubSub
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