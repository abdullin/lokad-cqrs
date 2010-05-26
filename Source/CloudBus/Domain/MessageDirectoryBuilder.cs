#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CloudBus.Domain
{
	public sealed class MessageDirectoryBuilder : IMessageDirectoryBuilder
	{


		IEnumerable<MessageMapping> _mappings;
		string _methodName;

		public MessageDirectoryBuilder(IEnumerable<MessageMapping> mappings, string methodName)
		{
			_mappings = mappings;
			_methodName = methodName;
		}

		public IMessageDirectory BuildDirectory(Func<MessageMapping, bool> filter)
		{
			var mappings = _mappings.Where(filter);

			var consumers = mappings
				.GroupBy(x => x.Consumer)
				.Select(x =>
					{
						var directs = x.Where(m => m.Direct).Select(m => m.Message).Distinct();
						var assignables = x
							.Select(m => m.Message)
							.Where(t => directs.Any(d => d.IsAssignableFrom(t)))
							.Distinct();

						return new ConsumerInfo(x.Key, assignables.ToArray());
					})
				.ToArray();


			var messages = mappings
				.ToLookup(x => x.Message)
				.ToArray(x =>
				{
					
					var domainConsumers = x
						.Where(t => t.Consumer != typeof(MessageMapping.BusSystem))
						.Where(t => t.Consumer != typeof(MessageMapping.BusNull))
						.ToArray();

					var info = new MessageInfo()
						{
							MessageType = x.Key,
							IsDomainMessage = x.Exists(t => t.Consumer != typeof (MessageMapping.BusSystem)),
							IsSystemMessage = x.Exists(t => t.Consumer == typeof (MessageMapping.BusSystem)),
							AllConsumers = domainConsumers.Select(m => m.Consumer).Distinct().ToArray(),
							DerivedConsumers = domainConsumers.Where(m => !m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
							DirectConsumers = domainConsumers.Where(m => m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
						};

					return info;
				});
			
			return new MessageDirectory(_methodName, consumers, messages);
		}
	}
}