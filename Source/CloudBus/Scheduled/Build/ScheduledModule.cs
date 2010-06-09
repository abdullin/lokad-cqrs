#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Autofac;
using Lokad;

namespace CloudBus.Scheduled.Build
{
	public sealed class ScheduledModule : Module
	{
		readonly ScheduledConfig _config;

		readonly IList<IScheduledTaskBuilder> _builders = new List<IScheduledTaskBuilder>();
		Func<ILifetimeScope, IScheduledTaskDispatcher> _dispatcher;

		public ScheduledModule()
		{
			_config = new ScheduledConfig
				{
					SleepBetweenCommands = 0.Seconds(),
					SleepOnEmptyChain = 1.Minutes(),
					SleepOnFailure = 2.Minutes(),
					IsolationLevel = IsolationLevel.ReadCommitted
				};

			WithDefaultDispatcher();
		}

		public ScheduledModule WithDefaultDispatcher()
		{
			_dispatcher = scope => new DefaultTaskDispatcher(scope);
			return this;
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (!_builders.Any())
			{
				throw new InvalidOperationException("No task builders have been registered for module");
			}
			
			var allTasks = _builders.SelectMany(t => t.BuildTasks()).ToArray();
			foreach (var info in allTasks)
			{
				builder.RegisterType(info.Task);
			}

			builder.RegisterType<ScheduledProcess>();
			builder.Register(c => ConfigureComponent(c, allTasks));
		}

		IBusProcess ConfigureComponent(IComponentContext context, ScheduledTaskInfo[] tasks)
		{
			var logger = context.Resolve<ILogProvider>().CreateLog<ScheduledModule>();
			logger.DebugFormat("{0} task available", tasks.Length);
			var dispatcher = _dispatcher(context.Resolve<ILifetimeScope>());

			return context.Resolve<ScheduledProcess>(
				TypedParameter.From(_config),
				TypedParameter.From(dispatcher),
				TypedParameter.From(tasks));
		}

		public ExpressionTaskBuilder<TTask> AdaptTasks<TTask>(Expression<Func<TTask,TimeSpan>> tasks)
		{
			var builder = new ExpressionTaskBuilder<TTask>(tasks);
			_builders.Add(builder);
			return builder;
		}
	}
}