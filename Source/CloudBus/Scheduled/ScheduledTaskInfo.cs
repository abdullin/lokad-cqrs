using System;
using System.Reflection;

namespace Lokad.Cqrs.Scheduled
{
	public sealed class ScheduledTaskInfo
	{
		public readonly string Name;
		public readonly Type Task;
		
		public readonly MethodInfo Invoker;

		public ScheduledTaskInfo(string name, Type task, MethodInfo invoker)
		{
			Name = name;
			Task = task;
			Invoker = invoker;
		}
	}
}