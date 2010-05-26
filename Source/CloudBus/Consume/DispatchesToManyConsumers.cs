#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using CloudBus.Domain;

namespace CloudBus.Consume
{
	public sealed class DispatchesToManyConsumers : IMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly IDictionary<Type, Type[]> _dictionary = new Dictionary<Type, Type[]>();
		readonly IMessageDirectory _messageDirectory;

		public DispatchesToManyConsumers(ILifetimeScope container, IMessageDirectory messageDirectory)
		{
			_container = container;
			_messageDirectory = messageDirectory;
		}

		public void Init()
		{
			foreach (var message in _messageDirectory.Messages)
			{
				if (message.AllConsumers.Length > 0)
				{
					_dictionary.Add(message.MessageType, message.AllConsumers);
				}
			}
		}

		public bool DispatchMessage(string topic, object message)
		{
			// we get failure if one of the subscribers fails
			Type[] consumerTypes;
			var type = message.GetType();
			if (_dictionary.TryGetValue(type, out consumerTypes))
			{
				using (var scope = _container.BeginLifetimeScope())
				{
					foreach (var consumerType in consumerTypes)
					{
						var consumer = scope.Resolve(consumerType);
						_messageDirectory.InvokeConsume(consumer, message);
					}
				}

				return true;
			}
			return false;
		}
	}
}