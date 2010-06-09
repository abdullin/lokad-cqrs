using System;
using System.Reflection;
using Autofac;

namespace CloudBus.Scheduled
{
	public sealed class DefaultTaskDispatcher : IScheduledTaskDispatcher
	{
		readonly ILifetimeScope _scope;

		public DefaultTaskDispatcher(ILifetimeScope scope)
		{
			_scope = scope;
		}

		public TimeSpan Execute(ScheduledTaskInfo info)
		{
			using (var scope = _scope.BeginLifetimeScope())
			{
				var instance = scope.Resolve(info.Task);
				try
				{
					return (TimeSpan) info.Invoker.Invoke(instance, new object[0]);
				}
				catch (TargetInvocationException e)
				{
					throw Throw.InnerExceptionWhilePreservingStackTrace(e);
				}
			}
		}
	}
}