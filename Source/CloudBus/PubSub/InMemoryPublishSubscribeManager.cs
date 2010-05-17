#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lokad.Quality;

namespace CloudBus.PubSub
{
	[UsedImplicitly]
	public sealed class InMemoryPublishSubscribeManager : IPublishSubscribeManager
	{
		readonly IDictionary<string, object> _subscriptions = new Dictionary<string, object>();


		ILookup<string, string> _directLookup;
		IList<RegexSubscription> _regexLookup;

		public InMemoryPublishSubscribeManager()
		{
			UpdateLookup();
		}


		public string[] GetSubscribers(string topic)
		{
			lock (_subscriptions)
			{
				return RetrieveSubscribersInternal(topic).ToArray();
			}
		}

		public void SubscribeDirect(string subscriptionId, string topic, string reference)
		{
			lock (_subscriptions)
			{
				_subscriptions[subscriptionId] = new DirectSubscription(topic, reference);
				UpdateLookup();
			}
		}

		public void SubscribeRegex(string subscriptionId, string regex, string reference)
		{
			lock (_subscriptions)
			{
				var r = new Regex(regex, RegexOptions.Compiled);
				_subscriptions[subscriptionId] = new RegexSubscription(r, reference);
				UpdateLookup();
			}
		}

		public void Unsubscribe(string subscriptionId)
		{
			lock (_subscriptions)
			{
				_subscriptions.Remove(subscriptionId);
				UpdateLookup();
			}
		}

		IEnumerable<string> RetrieveSubscribersInternal(string topic)
		{
			foreach (var regexSubscription in _regexLookup)
			{
				if (regexSubscription.Regex.IsMatch(topic))
				{
					yield return regexSubscription.Subscriber;
				}
				if (_directLookup.Contains(topic))
				{
					foreach (var subscriber in _directLookup[topic])
					{
						yield return subscriber;
					}
				}
			}
		}

		void UpdateLookup()
		{
			_directLookup = _subscriptions
				.Where(s => s.Value is DirectSubscription)
				.Select(s => (DirectSubscription) s.Value)
				.ToLookup(l => l.Topic, l => l.Subscriber);

			_regexLookup = _subscriptions
				.Where(s => s.Value is RegexSubscription)
				.ToArray(s => (RegexSubscription) s.Value);
		}

		sealed class DirectSubscription
		{
			public readonly string Subscriber;
			public readonly string Topic;

			public DirectSubscription(string topic, string subscriber)
			{
				Topic = topic;
				Subscriber = subscriber;
			}
		}

		sealed class RegexSubscription
		{
			public readonly Regex Regex;
			public readonly string Subscriber;

			public RegexSubscription(Regex regex, string subscriber)
			{
				Regex = regex;
				Subscriber = subscriber;
			}
		}
	}
}