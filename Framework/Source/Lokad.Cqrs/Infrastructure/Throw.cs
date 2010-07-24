#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using Autofac;

namespace Lokad.Cqrs
{
	public static class Throw
	{
		static readonly MethodInfo InternalPreserveStackTraceMethod;

		static Throw()
		{
			InternalPreserveStackTraceMethod = typeof (Exception).GetMethod("InternalPreserveStackTrace",
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static Exception InnerExceptionWhilePreservingStackTrace(TargetInvocationException e)
		{
			InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
			return e.InnerException;
		}
	}

	public static class ExtendIComponentContext
	{
		public static void WhenDisposed(this IComponentContext context, Action disposal)
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(new DisposableAction(disposal));
			builder.Update(context.ComponentRegistry);
		}
	}
}