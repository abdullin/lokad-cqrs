#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Queue;

namespace Lokad.Cqrs.Sender
{
	public sealed class SenderModule : Module
	{
		Action<ContainerBuilder> _builders = builder => { };

		public SenderModule DefaultToQueue(string queueName)
		{
			_builders += builder => builder.Register(c =>
				{
					var queue = c.Resolve<AzureQueueFactory>().GetWriteQueue(queueName);
					return new DefaultMessageClient(queue);
				}).SingleInstance().As<IMessageClient>();
			return this;
		}

		protected override void Load(ContainerBuilder builder)
		{
			_builders(builder);
		}
	}
}