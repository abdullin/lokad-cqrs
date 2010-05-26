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
	public sealed class MessageDirectoryBuilder
	{
		readonly HashSet<Func<DomainMessageMapping, bool>> _filters = new HashSet<Func<DomainMessageMapping, bool>>();

		public void AddConstraint(Func<DomainMessageMapping, bool> constraint)
		{
			_filters.Add(constraint);
		}

		public IMessageDirectory BuildDirectory(IEnumerable<DomainMessageMapping> domainMessageMappings, string methodName)
		{
			var mappings = _filters
				.Aggregate(domainMessageMappings, (current, func) => current.Where(func))
				.ToArray();

			var consumers = mappings
				.GroupBy(x => x.Consumer)
				.Select(x => new ConsumerInfo(x.Key, x.ToArray(e => e.Message)))
				.ToArray();

			mappings
				.Where(m => false == m.Message.IsAbstract)
				.SelectMany(t => t.Message.GetInterfaces());


			var messages = mappings
				.ToLookup(x => x.Message)
				.ToArray(x =>
					{
						var isDomainMessage = x.Exists(t => t.Consumer != typeof (DomainMessageMapping.BusSystem));
						var isSystemMessage = x.Exists(t => t.Consumer == typeof (DomainMessageMapping.BusSystem));
						var domainConsumers = x
							.Where(t => t.Consumer != typeof (DomainMessageMapping.BusSystem))
							.Where(t => t.Consumer != typeof (DomainMessageMapping.BusNull))
							.ToArray(t => t.Consumer);

						return new MessageInfo(x.Key, domainConsumers, isDomainMessage, isSystemMessage);
					});

			foreach (var message in messages)
			{
				var concreteTyp = message.MessageType;
				var implements = messages
					.Where(m => m.MessageType != concreteTyp)
					.Where(m => m.MessageType.IsAssignableFrom(concreteTyp))
					.ToArray();
				message.Implements = implements;
				message.DirectConsumers
					.Append(implements.SelectMany(i => i.DirectConsumers))
					.Distinct()
					.ToArray();
			}

			foreach (var message in messages)
			{
				message
					.DerivedConsumers = EnumImplementorTree(message)
						.Aggregate(new Type[0], (t, x) => t.Append(x.DirectConsumers));

				message.AllConsumers = message
					.DirectConsumers
					.Append(message.DerivedConsumers)
					.Distinct()
					.ToArray();
			}

			return new MessageDirectory(methodName, consumers, messages);
		}

		static IEnumerable<MessageInfo> EnumImplementorTree(MessageInfo info)
		{
			foreach (var messageInfo in info.Implements)
			{
				yield return messageInfo;

				foreach (var impl in EnumImplementorTree(messageInfo))
				{
					yield return impl;
				}
			}
		}
	}
}