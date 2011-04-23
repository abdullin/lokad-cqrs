#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Core.Directory
{
	/// <summary>
	/// Default implementation of the message directory builder
	/// </summary>
	public sealed class MessageDirectoryBuilder
	{
		readonly IEnumerable<MessageMapping> _mappings;
		readonly string _methodName;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDirectoryBuilder"/> class.
		/// </summary>
		/// <param name="mappings">The message mappings.</param>
		/// <param name="methodName">Name of the method for the invocation.</param>
		public MessageDirectoryBuilder(IEnumerable<MessageMapping> mappings, string methodName)
		{
			_mappings = mappings;
			_methodName = methodName;
		}

		public MessageDirectory BuildDirectory(Func<MessageMapping, bool> filter)
		{
			var mappings = _mappings.Where(filter);

			var consumers = mappings
				.GroupBy(x => x.Consumer)
				.Select(x =>
					{
						var directs = x
							.Where(m => m.Direct)
							.Select(m => m.Message)
							.Distinct();

						var assignables = x
							.Select(m => m.Message)
							.Where(t => directs.Any(d => d.IsAssignableFrom(t)))
							.Distinct();

						return new ConsumerInfo(x.Key, assignables.ToArray());
					})
				.ToArray();


			var messages = mappings
				.ToLookup(x => x.Message)
				.Select(x =>
					{
						var domainConsumers = x
							.Where(t => t.Consumer != typeof (MessageMapping.BusNull))
							.ToArray();

						return new MessageInfo
							{
								MessageType = x.Key,
								AllConsumers = domainConsumers.Select(m => m.Consumer).Distinct().ToArray(),
								DerivedConsumers = domainConsumers.Where(m => !m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
								DirectConsumers = domainConsumers.Where(m => m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
							};
					}).ToList();

			var includedTypes = new HashSet<Type>(messages.Select(m => m.MessageType));

			// message directory should still include all messages for the serializers
			var orphanedMessages = _mappings
				.Where(m => !includedTypes.Contains(m.Message))
				.Select(m => new MessageInfo
					{
						MessageType = m.Message,
						AllConsumers = Type.EmptyTypes,
						DerivedConsumers = Type.EmptyTypes,
						DirectConsumers = Type.EmptyTypes
					});

			messages.AddRange(orphanedMessages);

			return new MessageDirectory(_methodName, consumers, messages.ToArray());
		}
	}

	public sealed class MessageDirectoryFilter
	{
		readonly HashSet<Func<MessageMapping, bool>> _filters = new HashSet<Func<MessageMapping, bool>>();

		public bool DoesPassFilter(MessageMapping mapping)
		{
			return _filters.All(filter => filter(mapping));
		}

		/// <summary>
		/// Adds custom filters for <see cref="MessageMapping"/>, that will be used
		/// for configuring this message handler.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public MessageDirectoryFilter WhereMappings(Func<MessageMapping, bool> filter)
		{
			_filters.Add(filter);
			return this;
		}

		/// <summary>
		/// Adds filter to exclude all message mappings, where messages derive from the specified class
		/// </summary>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public MessageDirectoryFilter WhereMessagesAreNot<TMessage>()
		{
			return WhereMappings(mm => !typeof (TMessage).IsAssignableFrom(mm.Message));
		}

		/// <summary>
		/// Adds filter to include only message mappings, where messages derive from the specified class
		/// </summary>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public MessageDirectoryFilter WhereMessagesAre<TMessage>()
		{
			return WhereMappings(mm => typeof (TMessage).IsAssignableFrom(mm.Message));
		}

		/// <summary>
		/// Adds filter to include only message mappings, where consumers derive from the specified class
		/// </summary>
		/// <typeparam name="TConsumer">The type of the consumer.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public MessageDirectoryFilter WhereConsumersAre<TConsumer>()
		{
			return WhereMappings(mm => typeof (TConsumer).IsAssignableFrom(mm.Consumer));
		}

		/// <summary>
		/// Adds filter to exclude all message mappings, where consumers derive from the specified class
		/// </summary>
		/// <typeparam name="TConsumer">The type of the consumer.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public MessageDirectoryFilter WhereConsumersAreNot<TConsumer>()
		{
			return WhereMappings(mm => !typeof (TConsumer).IsAssignableFrom(mm.Consumer));
		}
	}
}