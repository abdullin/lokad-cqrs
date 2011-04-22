#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Core.Directory
{
	static class MessageReflectionUtil
	{
	


		public static MethodInfo ExpressConsumer<THandler>(Expression<Action<THandler>> expression)
		{
			if (expression == null) throw new ArgumentNullException("expression");

			if (expression.Body.NodeType != ExpressionType.Call)
				throw new ArgumentException("Expected 'Call' expression.");

			
			if (false == typeof (THandler).IsGenericType)
				throw new InvalidOperationException("Type should be a generic like 'IConsumeMessage<IMessage>'");

			var generic = typeof (THandler).GetGenericTypeDefinition();



			var methodInfo = ((MethodCallExpression)expression.Body).Method;

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