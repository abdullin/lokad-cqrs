#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Directory;
using Lokad.Cqrs.Durability;
using Lokad.Cqrs.Sender;

namespace Lokad.Cqrs.Dispatch
{
	///<summary>
	/// Dispatcher that sends a single event to multiple consumers within this worker.
	/// No transactions are used here.
	///</summary>
	public sealed class DispatchSingleEventToMultipleConsumers : ISingleThreadMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly MessageDirectory _directory;
		readonly IDictionary<Type, Type[]> _dispatcher = new Dictionary<Type, Type[]>();
		readonly SlidingDispatchMemory.DispatchMemory _dispatchMemory;

		public DispatchSingleEventToMultipleConsumers(ILifetimeScope container, MessageDirectory directory, SlidingDispatchMemory memory)
		{
			_container = container;
			_directory = directory;
			_dispatchMemory = memory.AcquireMemory(this);
		}

		public void Init()
		{
			foreach (var message in _directory.Messages)
			{
				if (message.AllConsumers.Length > 0)
				{
					_dispatcher.Add(message.MessageType, message.AllConsumers);
				}
			}
		}

		public void DispatchMessage(MessageEnvelope unpacked)
		{
			if (_dispatchMemory.DoWeRemember(unpacked.EnvelopeId))
				return;

			if (unpacked.Items.Length != 1)
				throw new InvalidOperationException("Batch message arrived to the shared scope. Are you batching events or dispatching commands to shared scope?");

			// we get failure if one of the subscribers fails
			Type[] consumerTypes;

			var item = unpacked.Items[0];
			if (_dispatcher.TryGetValue(item.MappedType, out consumerTypes))
			{
				using (var unit = _container.BeginLifetimeScope(MessageDispatch.UnitOfWorkTag))
				{
					foreach (var consumerType in consumerTypes)
					{
						using (var scope = unit.BeginLifetimeScope(MessageDispatch.ScopeTag))
						{
							var consumer = scope.Resolve(consumerType);
							_directory.InvokeConsume(consumer, item.Content);
						}
					}
				}
			}
			// else -> we don't have consumers. It's OK for the event

			_dispatchMemory.RegisterMemory(unpacked.EnvelopeId);
		}
	}

	public sealed class DispatchMessagesToRoute : ISingleThreadMessageDispatcher
	{
		readonly AzureWriteQueueFactory _queueFactory;
		Func<MessageEnvelope, string> _routerRule;

		public DispatchMessagesToRoute(AzureWriteQueueFactory queueFactory)
		{
			_queueFactory = queueFactory;
		}


		public void DispatchMessage(MessageEnvelope message)
		{
			var route = _routerRule(message);
			var queue = _queueFactory.GetWriteQueue(route);
			queue.ForwardMessage(message);
		}

		public void SpecifyRouter(Func<MessageEnvelope, string>  router)
		{
			_routerRule = router;
		}

		public void Init()
		{
			if (null == _routerRule)
			{
				throw new InvalidOperationException("Message router must be configured!");
			}
		}
	}
}