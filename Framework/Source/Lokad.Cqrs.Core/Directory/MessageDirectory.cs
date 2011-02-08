#region (c) 2010-2011 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Directory
{
	public sealed class MessageDirectory : IMessageDirectory, IKnowSerializationTypes
	{
		readonly ConsumerInfo[] _consumers;
		readonly MessageInfo[] _messages;
		readonly InvocationHandler _invocationHandler;
		readonly Type[] _knownTypes;
		
		public MessageDirectory(ConsumerInfo[] consumers, MessageInfo[] messages, InvocationHandler invocationHandler)
		{
			_consumers = consumers;
			_messages = messages;
			_invocationHandler = invocationHandler;

			_knownTypes = messages.Where(m => !m.MessageType.IsAbstract).ToArray(m => m.MessageType);
		}

		public ConsumerInfo[] Consumers
		{
			get { return _consumers; }
		}

		public MessageInfo[] Messages
		{
			get { return _messages; }
		}

		public void InvokeConsume(object consumer, object message, MessageAttributesContract attributes)
		{
			// could be cached here or precached at the constructor for the major types
			_invocationHandler.Invoke(consumer, message, attributes);
		}

		public IEnumerable<Type> GetKnownTypes()
		{
			return _knownTypes;
		}
	}
}