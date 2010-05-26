#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Quality;

namespace CloudBus.Domain
{
	[UsedImplicitly]
	public sealed class DomainAwareMessageProfiler : IMessageProfiler
	{
		readonly IDictionary<Type, GetInfoDelegate> _delegates;

		public DomainAwareMessageProfiler(IMessageDirectory directory)
		{
			_delegates = BuildFrom(directory);
		}


		public string GetReadableMessageInfo(object instance, string messageId)
		{
			GetInfoDelegate value;
			var type = instance.GetType();
			if (_delegates.TryGetValue(type, out value))
			{
				return value(instance, messageId);
			}
			return type.Name + " - " + messageId;
		}

		static IDictionary<Type, GetInfoDelegate> BuildFrom(IMessageDirectory directory)
		{
			var delegates = new Dictionary<Type, GetInfoDelegate>();
			foreach (var message in directory.Messages)
			{
				if (message.MessageType.IsInterface)
					continue;

				var type = message.MessageType;
				var hasStringOverride = type.GetMethod("ToString").DeclaringType != typeof (object);

				if (hasStringOverride)
				{
					delegates.Add(type, (o, id) => o.ToString());
				}
				else
				{
					delegates.Add(type, (o, id) => type.Name + " - " + id);
				}
			}
			return delegates;
		}

		delegate string GetInfoDelegate(object message, string messageId);
	}
}