#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lokad.Reflection;

namespace CloudBus.Domain
{
	static class MessageReflectionUtil
	{
		static MethodInfo InternalPreserveStackTraceMethod;


		static MessageReflectionUtil()
		{
			InternalPreserveStackTraceMethod = typeof (Exception).GetMethod("InternalPreserveStackTrace",
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static Exception InnerExceptionWhilePreservingStackTrace(TargetInvocationException e)
		{
			InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
			return e.InnerException;
		}

		[DebuggerNonUserCode]
		public static void InvokeConsume(object consumer, object msg, string name)
		{
			try
			{
				var type = consumer.GetType();
				var consume = type.GetMethod(name, new[] {msg.GetType()});
				consume.Invoke(consumer, new[] {msg});
			}
			catch (TargetInvocationException e)
			{
				throw InnerExceptionWhilePreservingStackTrace(e);
			}
		}


		public static MethodInfo ExpressConsumer<THandler>(Expression<Action<THandler>> expression)
		{
			if (false == typeof (THandler).IsGenericType)
				throw new InvalidOperationException("Type should be a generic like 'IConsumeMessage<IMessage>'");

			var generic = typeof (THandler).GetGenericTypeDefinition();

			var methodInfo = Express<THandler>.Method(expression);

			var parameters = methodInfo.GetParameters();
			if ((parameters.Length != 1)) //|| (parameters[0].ParameterType != typeof (string))
				throw new InvalidOperationException("Expression should consume object like: 'i => i.Consume(null)'");

			var method = generic
				.GetMethods()
				.Where(mi => mi.Name == methodInfo.Name)
				.Where(mi => mi.GetParameters().Length == 1)
				.Where(mi => mi.ContainsGenericParameters)
				.First();
			return method;
		}
	}
}