#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzurePartition.Sender
{
	public sealed class SendMessageModule : Module
	{
		Action<ContainerBuilder> _builders = builder => { };

		public SendMessageModule DefaultToQueue(string queueName)
		{
			_builders += builder => builder.Register(c =>
				{
					var queue = c.Resolve<IWriteQueueFactory>().GetWriteQueue(queueName);
					return new DefaultMessageSender(queue);
				}).SingleInstance().As<IMessageSender>();
			return this;
		}

		protected override void Load(ContainerBuilder builder)
		{
			_builders(builder);
		}
	}
}