using System;
using System.Collections.Generic;
using Autofac;
using Bus2.Domain;
using System.Linq;

namespace Bus2.Consume
{
	public sealed class DispatchesToManyConsumers : IMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly MessageInfo[] _events;
		readonly IDictionary<Type, Type[]> _dictionary = new Dictionary<Type, Type[]>();
		readonly IMessageDirectory _messageInvoker;

		public DispatchesToManyConsumers(ILifetimeScope container, MessageInfo[] events, IMessageDirectory messageInvoker)
		{
			_container = container;
			_events = events;
			_messageInvoker = messageInvoker;
		}

		public void Init()
		{
			foreach (var group in _events)
			{
				if (group.AllConsumers.Length > 0)
				{
					_dictionary.Add(group.MessageType, group.AllConsumers);
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
				using(var scope = _container.BeginLifetimeScope())
				{
					foreach (var consumerType in consumerTypes)
					{
						var consumer = scope.Resolve(consumerType);
						_messageInvoker.InvokeConsume(consumer, message);
					}
				}

				return true;
			}
			return false;
		}
	}
}