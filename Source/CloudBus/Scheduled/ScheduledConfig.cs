using System;
using System.Collections.Generic;
using Autofac;
using Lokad;
using System.Linq;

namespace Bus2.Scheduled
{
	public sealed class ScheduledConfig
	{
		public TimeSpan SleepBetweenCommands { get; set;}
		public TimeSpan SleepOnEmptyChain { get; set;}
		public TimeSpan SleepOnFailure { get; set; }
	}

	public sealed class ScheduledModule : Module
	{

		
		readonly ScheduledConfig _config;
		GetTasksDelegate _delegate;

		public delegate IEnumerable<ScheduledInfo> GetTasksDelegate(IComponentContext context);

		

		public ScheduledModule()
		{
			_config = new ScheduledConfig()
				{
					SleepBetweenCommands = 0.Seconds(),
					SleepOnEmptyChain = 1.Minutes(),
					SleepOnFailure = 2.Minutes()
				};

			_delegate = c => { throw new InvalidOperationException("Please configure location of the scheduled items");};
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<ScheduledProcess>()
				.As<IBusProcess>()
				.WithParameter(TypedParameter.From(_config));
		}

		public ScheduledModule AdaptTasksFromContainer<TTask>(Func<TTask, TimeSpan> adapt)
		{
			_delegate = context =>
				{
					return context
						.Resolve<IEnumerable<TTask>>()
						.Select(t => new ScheduledInfo(t.GetType().Name, new Func<TimeSpan>(() => adapt(t))));
				};
			return this;
		}
	}

}