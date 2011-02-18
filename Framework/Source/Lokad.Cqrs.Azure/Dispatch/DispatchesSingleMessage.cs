#region (c) 2010-2011 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Lokad.Cqrs.Directory;

namespace Lokad.Cqrs.Consume
{
	public sealed class DispatchesSingleMessage : IMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly IDictionary<Type, Type> _messageConsumers = new Dictionary<Type, Type>();
		readonly MessageDirectory _messageDirectory;


		public DispatchesSingleMessage(ILifetimeScope container, MessageDirectory messageDirectory)
		{
			_container = container;
			_messageDirectory = messageDirectory;
		}

		public void DispatchMessage(MessageEnvelope message)
		{
			// empty message, hm...
			if (message.Items.Length == 0)
				return;

			// verify that all consumers are available
			foreach (var item in message.Items)
			{
				if (!_messageConsumers.ContainsKey(item.MappedType))
				{
					throw new InvalidOperationException("Couldn't find consumer for " + item.MappedType);
				}
			}

			// we're dispatching them inside single lifetime scope
			// meaning same transaction,
			using (var scope = _container.BeginLifetimeScope())
			{
				foreach (var item in message.Items)
				{
					var consumerType = _messageConsumers[item.MappedType];
					{
						var consumer = scope.Resolve(consumerType);
						_messageDirectory.InvokeConsume(consumer, item.Content);
					}
				}
			}
		}

		static void ThrowIfCommandHasMultipleConsumers(IEnumerable<MessageInfo> commands)
		{
			var multipleConsumers = commands
				.Where(c => c.AllConsumers.Length > 1).ToArray(c => c.MessageType.FullName);

			if (multipleConsumers.Any())
			{
				throw new InvalidOperationException(
					"These messages have multiple consumers. Did you intend to declare them as events? " +
						multipleConsumers.Join(Environment.NewLine));
			}
		}

		public void Init()
		{
			ThrowIfCommandHasMultipleConsumers(_messageDirectory.Messages);
			foreach (var messageInfo in _messageDirectory.Messages)
			{
				if (messageInfo.AllConsumers.Length > 0)
				{
					_messageConsumers[messageInfo.MessageType] = messageInfo.AllConsumers[0];
				}
			}
		}
	}
}