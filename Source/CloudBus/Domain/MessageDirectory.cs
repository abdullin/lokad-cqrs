#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Lokad;

namespace CloudBus.Domain
{
	public sealed class MessageDirectory : IMessageDirectory
	{
		readonly string _consumeMethodName;
		readonly ConsumerInfo[] _consumers;
		readonly MessageInfo[] _messages;

		public MessageDirectory(string consumeMethodName, ConsumerInfo[] consumers, MessageInfo[] messages)
		{
			_consumeMethodName = consumeMethodName;
			_consumers = consumers;
			_messages = messages;
		}


		public ConsumerInfo[] Consumers
		{
			get { return _consumers; }
		}

		public MessageInfo[] Messages
		{
			get { return _messages; }
		}

		public void InvokeConsume(object consumer, object message)
		{
			MessageReflectionUtil.InvokeConsume(consumer, message, _consumeMethodName);
		}

		public IMessageDirectory WhereMessages(Func<MessageInfo, bool> filter)
		{
			var exclude = _messages.Where(mi => !filter(mi));
			if (!exclude.Any())
			{
				return this;
			}

			var include = _messages.Where(filter).ToArray();
			
			return new MessageDirectory(_consumeMethodName, _consumers, include);
		}

		static Maybe<ConsumerInfo> DropMessages(ConsumerInfo ci, IEnumerable<MessageInfo> excluding)
		{
			var dropping = excluding
				.SelectMany(mi => mi.Implements.Convert(x => x.MessageType).Append(mi.MessageType));

			var types = ci.MessageTypes.Except(dropping).ToArray();
			if (types.Length == 0)
				return Maybe<ConsumerInfo>.Empty;

			return new ConsumerInfo(ci.ConsumerType, types);
		}

		public IMessageDirectory WhereConsumers(Func<ConsumerInfo, bool> filter)
		{
			var exclude = _consumers.Where(mi => !filter(mi));
			if (!exclude.Any())
			{
				return this;
			}


			var include = _consumers.Where(filter).ToArray();
			return new MessageDirectory(_consumeMethodName, include, _messages);
		}
	}
}