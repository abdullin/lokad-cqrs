using System;
using System.Runtime.Serialization;

namespace Bus2.PubSub
{
	[DataContract]
	[Serializable]
	public sealed class SubscribeDirectMessage
	{
		[DataMember] public readonly string Topic;
		[DataMember] public readonly string Queue;
		[DataMember] public readonly string SubscriptionId;

		public SubscribeDirectMessage(string topic, string queue, string subscriptionId)
		{
			Topic = topic;
			Queue = queue;
			SubscriptionId = subscriptionId;
		}
	}
}