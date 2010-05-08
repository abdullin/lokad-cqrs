using System;
using System.Runtime.Serialization;

namespace Bus2.PubSub
{
	[DataContract]
	[Serializable]
	public sealed class SubscribeRegexMessage
	{
		[DataMember] public readonly string Regex;
		[DataMember] public readonly string Queue;
		[DataMember] public readonly string SubscriptionId;

		public SubscribeRegexMessage(string regex, string queue, string subscriptionId)
		{
			Regex = regex;
			Queue = queue;
			SubscriptionId = subscriptionId;
		}
	}
}