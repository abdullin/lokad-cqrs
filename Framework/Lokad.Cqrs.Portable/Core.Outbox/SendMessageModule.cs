#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Transport;
using System.Linq;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
	public sealed class SendMessageModule : BuildSyntaxHelper, IModule
	{
		readonly string _queueName;
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public SendMessageModule(string queueName)
		{
			_queueName = queueName;
		}

		

		DefaultMessageSender BuildDefaultMessageSender(IComponentContext c)
		{
			var factories = c.Resolve<IEnumerable<IQueueWriterFactory>>();

			var queues = new List<IQueueWriter>(1);
			foreach (var factory in factories)
			{
				IQueueWriter writer;
				if (factory.TryGetWriteQueue(_queueName, out writer))
				{
					queues.Add(writer);
				}
			}

			if (queues.Count == 0)
			{
				string message = string.Format("There are no queues for the '{0}'. Did you forget to register a factory?", _queueName);
				throw new InvalidOperationException(message);
			}
			if (queues.Count > 1)
			{
				string message = string.Format(
					"There are multiple queues for name '{0}'. Have you registered duplicate factories?", _queueName);
				throw new InvalidOperationException(message);
			}

			return new DefaultMessageSender(queues[0]);
		}

		public void Configure(IComponentRegistry componentRegistry)
		{
			Assert(!string.IsNullOrEmpty(_queueName), "Empty Queue name is set for SendMessageModule. Please set 'QueueName'.");
			Assert(ContainsQueuePrefix(_queueName), "Queue {0} should have queue provider name", _queueName);

			_builder.Register(BuildDefaultMessageSender).SingleInstance().As<IMessageSender>();
			_builder.Update(componentRegistry);
		}
	}
}