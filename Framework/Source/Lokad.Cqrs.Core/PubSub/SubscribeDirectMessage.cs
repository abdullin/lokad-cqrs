#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using Lokad.Quality;

namespace Lokad.Cqrs.PubSub
{
	[DataContract]
	[Serializable]
	public sealed class SubscribeDirectMessage
	{
		[DataMember(Order = 1)]
		public string Queue { get; private set; }
		[DataMember(Order = 2)]
		public string SubscriptionId { get; private set; }
		[DataMember(Order = 3)]
		public string Topic { get; private set; }

		[UsedImplicitly]
		SubscribeDirectMessage()
		{
		}


		public SubscribeDirectMessage(string topic, string queue, string subscriptionId)
		{
			Topic = topic;
			Queue = queue;
			SubscriptionId = subscriptionId;
		}
	}
}