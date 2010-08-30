#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;
using Lokad.Rules;

namespace Lokad.Reflection
{
	/// <summary>
	/// Helper class for the Expression-based strongly-typed reflection
	/// </summary>
	public static class Express
	{
		static Rule<LambdaExpression> LambdaIs(ExpressionType type)
		{
			return (expression, scope) =>
				{
					if (expression.Body.NodeType != type)
						scope.Error("Lambda expression must be of type {0}", type);
				};
		}

		/// <summary>
		/// Gets the <see cref="MethodInfo"/> 
		/// from the provided <paramref name="method"/>.
		/// </summary>
		/// <param name="method">The method expression.</param>
		/// <returns>method information</returns>
		public static MethodInfo MethodWithLambda(LambdaExpression method)
		{
			Enforce.Argument(() => method, LambdaIs(ExpressionType.Call));

			return ((MethodCallExpression) method.Body).Method;
		}

		/// <summary> Gets the <see cref="ConstructorInfo"/> from the 
		/// provided <paramref name="constructor"/> lambda. </summary>
		/// <param name="constructor">The constructor expression.</param>
		/// <returns>constructor information</returns>
		public static ConstructorInfo ConstructorWithLamda(LambdaExpression constructor)
		{
			Enforce.Argument(() => constructor, LambdaIs(ExpressionType.New));

			return ((NewExpression) constructor.Body).Constructor;
		}

		/// <summary> Gets the <see cref="MemberInfo"/> (field or property) 
		/// from the  provided <paramref name="member"/> </summary>
		/// <param name="member">The property expression.</param>
		/// <returns>member information</returns>
		public static MemberInfo MemberWithLambda(LambdaExpression member)
		{
			Enforce.Argument(() => member, LambdaIs(ExpressionType.MemberAccess));
			return ((MemberExpression) member.Body).Member;
		}

		/// <summary> Gets the <see cref="PropertyInfo"/> from the provided
		/// <paramref name="property"/> expression. </summary>
		/// <param name="property">The property expression.</param>
		/// <returns>property information</returns>
		public static PropertyInfo PropertyWithLambda(LambdaExpression property)
		{
			var value = MemberWithLambda(property);
			if (value.MemberType != MemberTypes.Property)
				throw Errors.InvalidOperation("Member must be a property reference");

			return (PropertyInfo) value;
		}

		/// <summary> Gets the <see cref="FieldInfo"/> from the provided 
		/// <paramref name="field"/> expression. </summary>
		/// <param name="field">The field expression.</param>
		/// <returns>field information</returns>
		public static FieldInfo FieldWithLambda(LambdaExpression field)
		{
			var value = MemberWithLambda(field);
			if (value.MemberType != MemberTypes.Field)
				throw Errors.InvalidOperation("Member must be a field reference");

			return (FieldInfo) value;
		}

		/// <summary> Gets the <see cref="MethodInfo"/> 
		/// from the provided <paramref name="method"/>.
		/// </summary>
		/// <param name="method">The method expression.</param>
		/// <returns>method information</returns>
		public static MethodInfo Method(Expression<Action> method)
		{
			return MethodWithLambda(method);
		}

		/// <summary>
		/// Gets the <see cref="ConstructorInfo"/> 
		/// from the provided <paramref name="constructorExpression"/>.
		/// </summary>
		/// <param name="constructorExpression">The constructor expression.</param>
		/// <returns>constructor information</returns>
		public static ConstructorInfo Constructor<T>(Expression<Func<T>> constructorExpression)
		{
			return ConstructorWithLamda(constructorExpression);
		}

		/// <summary> Gets the <see cref="PropertyInfo"/> from the provided
		/// <paramref name="property"/> expression. </summary>
		/// <param name="property">The property expression.</param>
		/// <returns>property information</returns>
		public static PropertyInfo Property<T>(Expression<Func<T>> property)
		{
			return PropertyWithLambda(property);
		}

		/// <summary> Gets the <see cref="FieldInfo"/> from the provided 
		/// <paramref name="field"/> expression. </summary>
		/// <param name="field">The field expression.</param>
		/// <returns>field information</returns>
		public static FieldInfo Field<T>(Expression<Func<T>> field)
		{
			return FieldWithLambda(field);
		}
	}

	/// <summary>
	/// Helper class for the Expression-based strongly-typed reflection
	/// </summary>
	public static class Express<TTarget>
	{
		/// <summary> Gets the <see cref="MethodInfo"/> from 
		/// the provided <paramref name="method"/> expression. </summary>
		/// <param name="method">The expression.</param>
		/// <returns>method information</returns>
		/// <seealso cref="Express.MethodWithLambda"/>
		public static MethodInfo Method(Expression<Action<TTarget>> method)
		{
			return Express.MethodWithLambda(method);
		}

		/// <summary> Gets the <see cref="PropertyInfo"/> from the provided
		/// <paramref name="property"/> expression. </summary>
		/// <param name="property">The property expression.</param>
		/// <returns>property information</returns>
		/// <seealso cref="Express.MemberWithLambda"/>
		/// <seealso cref="Express.PropertyWithLambda"/>
		public static PropertyInfo Property<T>(Expression<Func<TTarget, T>> property)
		{
			return Express.PropertyWithLambda(property);
		}

		/// <summary> Gets the <see cref="FieldInfo"/> from the provided 
		/// <paramref name="field"/> expression. </summary>
		/// <param name="field">The field expression.</param>
		/// <returns>field information</returns>
		/// <seealso cref="Express.MemberWithLambda"/>
		/// <seealso cref="Express.FieldWithLambda"/>
		public static FieldInfo Field<T>(Expression<Func<TTarget, T>> field)
		{
			return Express.FieldWithLambda(field);
		}
	}
}