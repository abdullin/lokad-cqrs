#region (c) 2010-2011 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Core.Evil;

namespace Lokad.Cqrs.Core.Directory
{
	public sealed class MessageDirectory : IKnowSerializationTypes
	{
		readonly string _consumeMethodName;
		readonly ConsumerInfo[] _consumers;
		readonly ICollection<MessageInfo> _messages;
		readonly Type[] _knownTypes;

		public MessageDirectory(string consumeMethodName, ConsumerInfo[] consumers, ICollection<MessageInfo> messages)
		{
			_consumeMethodName = consumeMethodName;
			_consumers = consumers;
			_messages = messages;

			_knownTypes = messages
				.Where(m => false == m.MessageType.IsAbstract)
				.Select(m => m.MessageType)
				.ToArray();
		}

		public ICollection<ConsumerInfo> Consumers
		{
			get { return _consumers; }
		}

		public ICollection<MessageInfo> Messages
		{
			get { return _messages; }
		}

		public void InvokeConsume(object consumer, object message)
		{
			InvocationUtil.InvokeConsume(consumer, message, _consumeMethodName);
		}

		public IEnumerable<Type> GetKnownTypes()
		{
			return _knownTypes;
		}
	}
}