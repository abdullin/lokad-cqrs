#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using CloudBus.Queue;

namespace CloudBus.Sender.Build
{
	public sealed class SenderModule : Module
	{
		Action<ContainerBuilder> _builders = builder => { };

		public SenderModule DefaultToQueue(string queueName)
		{
			_builders += builder => builder.Register(c =>
				{
					var queue = c.Resolve<IQueueManager>().GetWriteQueue(queueName);
					return new DefaultBusSender(queue);
				}).SingleInstance().As<IBusSender>();
			return this;
		}

		protected override void Load(ContainerBuilder builder)
		{
			_builders(builder);
		}
	}
}