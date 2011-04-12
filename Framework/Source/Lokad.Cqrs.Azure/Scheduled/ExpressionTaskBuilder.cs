using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Extensions;


namespace Lokad.Cqrs.Scheduled
{
	public sealed class ExpressionTaskBuilder<TTask> : IScheduledTaskBuilder
	{
		readonly MethodInfo _info;
		readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
		readonly Filter<Type> _taskFilter = new Filter<Type>();

		public Func<Type, string> Naming = type => type.Name;

		bool _allowEmptyBuilder = false;

		public ExpressionTaskBuilder<TTask> AllowEmptyBuilder()
		{
			_allowEmptyBuilder = true;
			return this;
		}

		public ExpressionTaskBuilder<TTask> WithFilter(Func<Type, bool> filter)
		{
			_taskFilter.AddFilter(filter);
			return this;
		}

		public ExpressionTaskBuilder<TTask> InCurrentAssembly()
		{
			_assemblies.Add(Assembly.GetCallingAssembly());
			return this;
		}

		public ExpressionTaskBuilder<TTask> InAssembly(Assembly assembly)
		{
			_assemblies.Add(assembly);
			return this;
		}

		public ExpressionTaskBuilder<TTask> InAssemblies(IEnumerable<Assembly> assembly)
		{
			foreach (var a in assembly)
			{
				_assemblies.Add(a);
			}
			return this;
		}

		public ExpressionTaskBuilder<TTask> InAssemblyOf<TImplementation>()
		{
			var assembly = typeof (TImplementation).Assembly;
			if (null == assembly)
			{
				throw new InvalidOperationException(string.Format("Assembly of {0} is null", typeof(TImplementation)));
			}
			_assemblies.Add(assembly);
			return this;
		}

		public ExpressionTaskBuilder(Expression<Func<TTask, TimeSpan>> adapter)
		{
			if (adapter == null) throw new ArgumentNullException("adapter");

			if (adapter.Body.NodeType != ExpressionType.Call)
				throw new ArgumentException("Expected 'Call' expression");

			_info = ((MethodCallExpression)adapter.Body).Method;
		}

		public IEnumerable<ScheduledTaskInfo> BuildTasks()
		{
			var scheduledTaskInfos = _assemblies
				.SelectMany(a => a.GetTypes())
				.Where(t => !t.IsAbstract)
				.Where(t => typeof (TTask).IsAssignableFrom(t))
				.Where(_taskFilter.BuildFilter())
				.Select(t => new ScheduledTaskInfo(Naming(t), t, _info))
				.ToArray();

			if (scheduledTaskInfos.Length == 0 && !_allowEmptyBuilder)
			{
				throw new InvalidOperationException("Task builder is empty. Have you missed specifications?");
			}
			return scheduledTaskInfos;
		}
	}
}