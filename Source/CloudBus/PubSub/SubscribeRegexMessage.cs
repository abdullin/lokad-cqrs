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
	public sealed class SubscribeRegexMessage
	{
		[DataMember] public readonly string Queue;
		[DataMember] public readonly string Regex;
		[DataMember] public readonly string SubscriptionId;

		public SubscribeRegexMessage(string regex, string queue, string subscriptionId)
		{
			Regex = regex;
			Queue = queue;
			SubscriptionId = subscriptionId;
		}
	}
}