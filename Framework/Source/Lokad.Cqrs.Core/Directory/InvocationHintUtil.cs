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

namespace Lokad.Cqrs.Directory
{
	
	public sealed class InvocationHint
	{
		public readonly Type ConsumerTypeDefinition;
		public readonly Maybe<Type> MessageContextType;
		public readonly string MethodName;
		public readonly bool HasContext;
		public readonly Type MessageInterface;

		public InvocationHint(Type consumerTypeDefinition, Maybe<Type> messageContextType, string methodName, Type messageinterface)
		{
			ConsumerTypeDefinition = consumerTypeDefinition;
			MessageContextType = messageContextType;
			MethodName = methodName;
			HasContext = messageContextType.HasValue;
			MessageInterface = messageinterface;
		}

		public static InvocationHint FromConsumerSample<THandler>(Expression<Action<THandler>> expression)
		{
			if (false == typeof(THandler).IsGenericType)
				throw new InvalidOperationException("Type should be a generic like 'IConsumeMessage<IMessage>'");

			var arguments = typeof (THandler).GetGenericArguments();

			if (arguments.Length != 1)
				throw new InvalidOperationException("Expected one generic argument");

			if (!arguments[0].IsInterface)
				throw new InvalidOperationException("Expected interface message base");

			var messageInterface = arguments[0];


			var interfaceTypedMethod = Express<THandler>.Method(expression);
			var parameters = interfaceTypedMethod.GetParameters();
			if ((parameters.Length < 1) || (parameters.Length > 2)) //|| (parameters[0].ParameterType != typeof (string))
				throw new InvalidOperationException("Expression should consume object like: 'i => i.Consume(null)' or 'i => i.Consume(null,null))'");


			var declaringGenericInterface = typeof(THandler).GetGenericTypeDefinition();
			var method = declaringGenericInterface
				.GetMethods()
				.Where(mi => mi.Name == interfaceTypedMethod.Name)
				.Where(mi => mi.ContainsGenericParameters)
				.Where(mi =>
				{
					var length = mi.GetParameters().Length;
					return length == parameters.Length;
				})
				.FirstOrEmpty()
				.ExposeException("Can't find generic method definition");

			var genericParameters = method.GetParameters();

			if (genericParameters.Length == 2)
			{
				return new InvocationHint(declaringGenericInterface, genericParameters[1].ParameterType, method.Name, messageInterface);
			}
			return new InvocationHint(declaringGenericInterface, Maybe<Type>.Empty, method.Name, messageInterface);
		}
	}
}