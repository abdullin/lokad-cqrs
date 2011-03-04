#region (c) 2010-2011 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Directory;
using Lokad.Cqrs.Durability;

namespace Lokad.Cqrs.Dispatch
{
	/// <summary>
	/// Dispatch command batches to a single consumer. Uses sliding cache to 
	/// reduce message duplication
	/// </summary>
	public sealed class DispatchCommandBatchToSingleConsumer : ISingleThreadMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly IDictionary<Type, Type> _messageConsumers = new Dictionary<Type, Type>();
		readonly MessageDirectory _messageDirectory;
		readonly MessageDuplicationMemory _memory;

		public DispatchCommandBatchToSingleConsumer(ILifetimeScope container, MessageDirectory messageDirectory, MessageDuplicationManager manager)
		{
			_container = container;
			_messageDirectory = messageDirectory;
			_memory = manager.GetOrAdd(this);
		}

		public void DispatchMessage(MessageEnvelope message)
		{
			// already dispatched
			if (_memory.DoWeRemember(message.EnvelopeId))
				return;

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

			using (var unit = _container.BeginLifetimeScope(MessageDispatch.UnitOfWorkTag))
			{
				foreach (var item in message.Items)
				{
					// we're dispatching them inside single lifetime scope
					// meaning same transaction,
					using (var scope = unit.BeginLifetimeScope(MessageDispatch.ScopeTag))
					{
						var consumerType = _messageConsumers[item.MappedType];
						{
							var consumer = scope.Resolve(consumerType);
							_messageDirectory.InvokeConsume(consumer, item.Content);
						}
					}
				}
			}
			_memory.Memorize(message.EnvelopeId);
		}

	

		public void Init()
		{
			MessageDispatch.ThrowIfCommandHasMultipleConsumers(_messageDirectory.Messages);
			foreach (var messageInfo in _messageDirectory.Messages)
			{
				if (messageInfo.AllConsumers.Length > 0)
				{
					_messageConsumers[messageInfo.MessageType] = messageInfo.AllConsumers[0];
				}
			}
		}
	}

	//public sealed class DispatchCommandMessagesInTransaction : IMessageDispatcher
	//{
	//    readonly ILifetimeScope _container;
	//    readonly IDictionary<Type, Type> _messageConsumers = new Dictionary<Type, Type>();
	//    readonly MessageDirectory _messageDirectory;

	//    public DispatchCommandMessagesInTransaction(ILifetimeScope container, MessageDirectory messageDirectory)
	//    {
	//        _container = container;
	//        _messageDirectory = messageDirectory;
	//    }

	//    public void DispatchMessage(MessageEnvelope message)
	//    {
	//        // empty message, hm...
	//        if (message.Items.Length == 0)
	//            return;

	//        // verify that all consumers are available
	//        foreach (var item in message.Items)
	//        {
	//            if (!_messageConsumers.ContainsKey(item.MappedType))
	//            {
	//                throw new InvalidOperationException("Couldn't find consumer for " + item.MappedType);
	//            }
	//        }

	//        using (var unit = _container.BeginLifetimeScope(MessageDispatch.UnitOfWorkTag))
	//        {
	//            foreach (var item in message.Items)
	//            {
	//                // we're dispatching them inside single lifetime scope
	//                // meaning same transaction,
	//                using (var scope = unit.BeginLifetimeScope(MessageDispatch.ScopeTag))
	//                {
	//                    var consumerType = _messageConsumers[item.MappedType];
	//                    {
	//                        var consumer = scope.Resolve(consumerType);
	//                        _messageDirectory.InvokeConsume(consumer, item.Content);
	//                    }
	//                }
	//            }
	//        }
	//    }

	//    public void Init()
	//    {
	//        MessageDispatch.ThrowIfCommandHasMultipleConsumers(_messageDirectory.Messages);
	//        foreach (var messageInfo in _messageDirectory.Messages)
	//        {
	//            if (messageInfo.AllConsumers.Length > 0)
	//            {
	//                _messageConsumers[messageInfo.MessageType] = messageInfo.AllConsumers[0];
	//            }
	//        }
	//    }


	//}
}