using System;
using System.Reflection;

namespace Lokad.Cqrs.Feature.Recurrent
{
	public sealed class RecurrentTaskInfo
	{
		public readonly string Name;
		public readonly Type Task;
		
		public readonly MethodInfo Invoker;

		public RecurrentTaskInfo(string name, Type task, MethodInfo invoker)
		{
			Name = name;
			Task = task;
			Invoker = invoker;
		}
	}
}