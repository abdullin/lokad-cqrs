#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Lokad;

namespace CloudBus.Scheduled
{
	public sealed class ScheduledModule : Module
	{
		readonly ScheduledConfig _config;
		GetTasksDelegate _delegate;


		public ScheduledModule()
		{
			_config = new ScheduledConfig
				{
					SleepBetweenCommands = 0.Seconds(),
					SleepOnEmptyChain = 1.Minutes(),
					SleepOnFailure = 2.Minutes()
				};

			_delegate = c => { throw new InvalidOperationException("Please configure location of the scheduled items"); };
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<ScheduledProcess>()
				.As<IBusProcess>()
				.WithParameter(TypedParameter.From(_config));
			builder
				.Register(c => _delegate(c));
		}

		public ScheduledModule AdaptTasksFromContainer<TTask>(Func<TTask, TimeSpan> adapt)
		{
			_delegate = context =>
				{
					return context
						.Resolve<IEnumerable<TTask>>()
						.Select(t => new ScheduledInfo(t.GetType().Name, () => adapt(t)));
				};
			return this;
		}

		delegate IEnumerable<ScheduledInfo> GetTasksDelegate(IComponentContext context);
	}
}