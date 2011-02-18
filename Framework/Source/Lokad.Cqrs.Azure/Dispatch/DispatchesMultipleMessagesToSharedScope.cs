#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Directory;
using Lokad.Cqrs.Domain;
using Lokad.Cqrs.Queue;

namespace Lokad.Cqrs.Consume
{
	public sealed class DispatchesMultipleMessagesToSharedScope : IMessageDispatcher
	{
		readonly ILifetimeScope _container;
		readonly MessageDirectory _directory;
		readonly IDictionary<Type, Type[]> _dispatcher = new Dictionary<Type, Type[]>();

		public DispatchesMultipleMessagesToSharedScope(ILifetimeScope container, MessageDirectory directory)
		{
			_container = container;
			_directory = directory;
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
			// we get failure if one of the subscribers fails
			Type[] consumerTypes;
			
			if (_dispatcher.TryGetValue(unpacked.ContractType, out consumerTypes))
			{
				using (var scope = _container.BeginLifetimeScope())
				{
					foreach (var consumerType in consumerTypes)
					{
						var consumer = scope.Resolve(consumerType);
						_directory.InvokeConsume(consumer, unpacked.Content);
					}
				}
			}
		}
	}
}