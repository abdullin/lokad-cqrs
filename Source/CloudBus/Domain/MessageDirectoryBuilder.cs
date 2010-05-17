#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CloudBus.Domain
{
	public sealed class MessageDirectoryBuilder
	{
		readonly List<DomainMessageConsumerMapping> _mappings = new List<DomainMessageConsumerMapping>();

		static IEnumerable<Type> GetConsumedMessages(Type type, Type consumerTypeDefinition)
		{
			var interfaces = type
				.GetInterfaces()
				.Where(i => i.IsGenericType)
				.Where(t => t.GetGenericTypeDefinition() == consumerTypeDefinition);

			foreach (var consumerInterface in interfaces)
			{
				yield return consumerInterface.GetGenericArguments()[0];
			}
		}


		public IMessageDirectory BuildDirectory(MethodInfo consumer)
		{
			var consumers = _mappings
				.GroupBy(x => x.Consumer)
				.Select(x => new ConsumerInfo(x.Key, x.ToArray(e => e.Message)))
				.ToArray();

			_mappings
				.Where(m => false == m.Message.IsAbstract)
				.SelectMany(t => t.Message.GetInterfaces());


			var messages = _mappings
				.ToLookup(x => x.Message)
				.ToArray(x =>
					{
						var isDomainMessage = x.Exists(t => t.Consumer != typeof (BusSystem));
						var isSystemMessage = x.Exists(t => t.Consumer == typeof (BusSystem));
						var domainConsumers = x
							.Where(t => t.Consumer != typeof (BusSystem))
							.Where(t => t.Consumer != typeof (BusNull))
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

			return new MessageDirectory(consumer.Name, consumers, messages);
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

		public void LoadDomainMessagesAndConsumers(IEnumerable<Assembly> assemblies, Type consumingTypeDefition,
			Func<Type, bool> messageSelector)
		{
			var types = assemblies
				.SelectMany(a => a.GetExportedTypes())
				.ToList();

			var mappings = types
				.Where(t => false == t.IsAbstract)
				.SelectMany(handler =>
					GetConsumedMessages(handler, consumingTypeDefition)
						.Select(c => new DomainMessageConsumerMapping(c, handler)))
				.ToList();

			_mappings.AddRange(mappings);

			// add unmapped messages
			var listed = mappings.ToSet(m => m.Message);


			var unmapped = types
				.Where(messageSelector)
				.Where(m => false == listed.Contains(m))
				.Select(c => new DomainMessageConsumerMapping(c, typeof (BusNull)));

			_mappings.AddRange(unmapped);
		}

		public void LoadSystemMessages()
		{
			var mappings = GetType()
				.Assembly
				.GetTypes()
				.Where(t => t.IsPublic)
				.Where(t => t.IsDefined(typeof (DataContractAttribute), false))
				.Select(c => new DomainMessageConsumerMapping(c, typeof (BusSystem)));

			_mappings.AddRange(mappings);
		}

		static class BusNull
		{
		}

		static class BusSystem
		{
		}

		sealed class DomainMessageConsumerMapping
		{
			public readonly Type Consumer;
			public readonly Type Message;

			public DomainMessageConsumerMapping(Type message, Type consumer)
			{
				Message = message;
				Consumer = consumer;
			}
		}
	}
}