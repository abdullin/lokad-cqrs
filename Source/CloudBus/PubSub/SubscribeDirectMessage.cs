#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace CloudBus.PubSub
{
	[DataContract]
	[Serializable]
	public sealed class SubscribeDirectMessage
	{
		[DataMember] public readonly string Queue;
		[DataMember] public readonly string SubscriptionId;
		[DataMember] public readonly string Topic;

		public SubscribeDirectMessage(string topic, string queue, string subscriptionId)
		{
			Topic = topic;
			Queue = queue;
			SubscriptionId = subscriptionId;
		}
	}
}