using System;
using System.Collections.Generic;
using Autofac;
using Bus2.Domain;
using Lokad;
using System.Linq;

namespace Bus2.Consume
{
	public sealed class DispatchesToSingleConsumer : IMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly MessageInfo[] _messages;
		readonly IDictionary<Type, Type> _messageConsumers = new Dictionary<Type, Type>();
		readonly IMessageDirectory _messageInvoker;


		public DispatchesToSingleConsumer(ILifetimeScope container, MessageInfo[] messages, IMessageDirectory messageInvoker)
		{
			_container = container;
			_messages = messages;
			_messageInvoker = messageInvoker;
		}


		public void Init()
		{
			foreach (var messageInfo in _messages)
			{
				Enforce.That(messageInfo.DirectConsumers.Length == 1);
				_messageConsumers[messageInfo.MessageType] = messageInfo.DirectConsumers[0];
			}
		}

		public bool DispatchMessage(string topic, object message)
		{
			Type consumerType;
			var type = message.GetType();
			if (_messageConsumers.TryGetValue(type, out consumerType))
			{
				using (var scope = _container.BeginLifetimeScope())
				{
					var consumer = scope.Resolve(consumerType);
					_messageInvoker.InvokeConsume(consumer, message);
				}
				
				return true;
			}
			return false;
		}
	}
}