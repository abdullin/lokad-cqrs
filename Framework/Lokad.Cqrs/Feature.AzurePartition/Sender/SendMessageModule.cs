#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Core.Transport;
using System.Linq;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
	public sealed class SendMessageModule : Module
	{
		public string QueueName { get; set; }

		

		protected override void Load(ContainerBuilder builder)
		{
			if (string.IsNullOrEmpty(QueueName))
			{
				throw new InvalidOperationException("Empty Queue name is set for SendMessageModule. Please set 'QueueName'.");
			}

			builder.Register(BuildDefaultMessageSender).SingleInstance().As<IMessageSender>();
			
		}

		DefaultMessageSender BuildDefaultMessageSender(IComponentContext c)
		{
			var factories = c.Resolve<IEnumerable<IQueueWriterFactory>>();

			var queues = new List<IQueueWriter>(1);
			foreach (var factory in factories)
			{
				IQueueWriter writer;
				if (factory.TryGetWriteQueue(QueueName, out writer))
				{
					queues.Add(writer);
				}
			}

			if (queues.Count == 0)
			{
				string message = string.Format("There are no queues for the '{0}'. Did you forget to register a factory?", QueueName);
				throw new InvalidOperationException(message);
			}
			if (queues.Count > 1)
			{
				string message = string.Format(
					"There are multiple queues for name '{0}'. Have you registered duplicate factories?", QueueName);
				throw new InvalidOperationException(message);
			}

			return new DefaultMessageSender(queues[0]);
		}
	}
}