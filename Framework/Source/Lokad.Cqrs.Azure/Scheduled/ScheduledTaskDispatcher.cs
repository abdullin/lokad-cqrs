#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using Autofac;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Scheduled
{
	/// <summary>
	/// Simple scheduled task dispatcher. For the transactional version see <see cref="ScheduledTaskDispatcherWithTransactions"/>.
	/// </summary>
	public sealed class ScheduledTaskDispatcher : IScheduledTaskDispatcher
	{
		readonly ILifetimeScope _scope;

		public ScheduledTaskDispatcher(ILifetimeScope scope)
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
					throw Errors.Inner(e);
				}
			}
		}
	}
}