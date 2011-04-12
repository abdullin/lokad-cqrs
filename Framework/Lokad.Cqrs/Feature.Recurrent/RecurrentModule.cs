#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac;
using Lokad.Cqrs.Feature.DefaultInterfaces;

namespace Lokad.Cqrs.Feature.Recurrent
{
	public sealed class RecurrentModule : Module
	{
		readonly RecurrentConfig _config;
		readonly IList<IRecurrentTaskBuilder> _builders = new List<IRecurrentTaskBuilder>();
		Func<ILifetimeScope, IRecurrentTaskDispatcher> _dispatcher;

		public RecurrentModule()
		{
			_config = new RecurrentConfig
				{
					SleepBetweenCommands = 0.Seconds(),
					SleepOnEmptyChain = 1.Seconds(),
					SleepOnFailure = 1.Seconds(),
				};

			WithDefaultDispatcher();
		}
		

		/// <summary>
		/// Sets the amount of time to sleep between the tasks/commands.
		/// </summary>
		/// <param name="sleepInterval">The sleep interval.</param>
		/// <returns>same module for inlining configs</returns>
		public RecurrentModule SleepBetweenCommands(TimeSpan sleepInterval)
		{
			_config.SleepBetweenCommands = sleepInterval;
			return this;
		}

		/// <summary>
		/// Sets the amount of time to sleep between the exceptions.
		/// </summary>
		/// <param name="timeToSleepBetweenExceptions">The time to sleep between exceptions.</param>
		/// <returns>same module for inlining configs</returns>
		public RecurrentModule SleepOnFailure(TimeSpan timeToSleepBetweenExceptions)
		{
			_config.SleepOnFailure = timeToSleepBetweenExceptions;
			return this;
		}

		/// <summary>
		/// Registers the default dispatcher (actually called by default)
		/// </summary>
		/// <returns></returns>
		public RecurrentModule WithDefaultDispatcher()
		{
			_dispatcher = scope => new RecurrentTaskDispatcherWithTransactions(scope);
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

			builder.RegisterType<RecurrentProcess>();
			builder.Register(c => ConfigureComponent(c, allTasks));
		}

		IEngineProcess ConfigureComponent(IComponentContext context, RecurrentTaskInfo[] tasks)
		{
			var dispatcher = _dispatcher(context.Resolve<ILifetimeScope>());

			return context.Resolve<RecurrentProcess>(
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

		public ExpressionTaskBuilder<IScheduledTask> WithDefaultInterfaces()
		{
			return AdaptTasks<IScheduledTask>(t => t.Execute());
		}
	}
}