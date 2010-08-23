#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Lokad.Quality;
using Lokad.Reflection;
using Lokad.Rules;

namespace Lokad
{
	/// <summary>
	/// Helper class allows to follow the principles defined by Microsoft P&amp;P team.
	/// </summary>
	public static class Enforce
	{
		// refactored methods go here.


		/// <summary>
		/// <para>Throws exception if the provided object is null. </para>
		/// <code>Enforce.Argument(() => args);</code> 
		/// </summary>
		/// <typeparam name="TValue">type of the class to check</typeparam>
		/// <param name="argumentReference">The argument reference to check.</param>
		/// <exception cref="ArgumentNullException">If the class reference is null.</exception>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		public static void Argument<TValue>(Func<TValue> argumentReference) where TValue : class
		{
			if (null == argumentReference())
				throw Errors.ArgumentNull(argumentReference);
		}

		/// <summary>
		/// 	<para>Throws exception if one of the provided objects is null. </para>
		/// 	<code>Enforce.Arguments(() =&gt; controller, () =&gt; service);</code>
		/// </summary>
		/// <param name="first">The first argument to check for</param>
		/// <param name="second">The second argument to check for.</param>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		public static void Arguments<T1, T2>(Func<T1> first, Func<T2> second)
			where T1 : class
			where T2 : class

		{
			if (null == first())
				throw Errors.ArgumentNull(first);

			if (null == second())
				throw Errors.ArgumentNull(second);
		}


		/// <summary>
		/// 	<para>Throws exception if one of the provided objects is null. </para>
		/// 	<code>Enforce.Arguments(() =&gt; controller, () =&gt; service, () =&gt; parameters);</code>
		/// </summary>
		/// <typeparam name="T1">The type of the first argument.</typeparam>
		/// <typeparam name="T2">The type of the second argument.</typeparam>
		/// <typeparam name="T3">The type of the third argument.</typeparam>
		/// <param name="first">The first argument to check</param>
		/// <param name="second">The second argument to check.</param>
		/// <param name="third">The third argument to check.</param>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		public static void Arguments<T1, T2, T3>(Func<T1> first, Func<T2> second, Func<T3> third)
			where T1 : class
			where T2 : class
			where T3 : class
		{
			if (null == first())
				throw Errors.ArgumentNull(first);

			if (null == second())
				throw Errors.ArgumentNull(second);

			if (null == third())
				throw Errors.ArgumentNull(third);
		}

		/// <summary>
		/// 	<para>Throws exception if one of the provided objects is null. </para>
		/// 	<code>Enforce.Arguments(() =&gt; controller, () =&gt; service, () =&gt; parameters);</code>
		/// </summary>
		/// <typeparam name="T1">The type of the first argument.</typeparam>
		/// <typeparam name="T2">The type of the second argument.</typeparam>
		/// <typeparam name="T3">The type of the third argument.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument.</typeparam>
		/// <param name="first">The first argument to check.</param>
		/// <param name="second">The second argument to check.</param>
		/// <param name="third">The third argument to check.</param>
		/// <param name="fourth">The fourth argument to check.</param>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		public static void Arguments<T1, T2, T3, T4>(Func<T1> first, Func<T2> second, Func<T3> third, Func<T4> fourth)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
		{
			if (null == first())
				throw Errors.ArgumentNull(first);

			if (null == second())
				throw Errors.ArgumentNull(second);

			if (null == third())
				throw Errors.ArgumentNull(third);

			if (null == fourth())
				throw Errors.ArgumentNull(fourth);
		}

		/// <summary>
		/// 	<para>Throws exception if one of the provided objects is null. </para>
		/// 	<code>Enforce.Arguments(() =&gt; controller, () =&gt; service, () =&gt; parameters);</code>
		/// </summary>
		/// <typeparam name="T1">The type of the first argument.</typeparam>
		/// <typeparam name="T2">The type of the second argument.</typeparam>
		/// <typeparam name="T3">The type of the third argument.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument.</typeparam>
		/// <param name="first">The first argument to check.</param>
		/// <param name="second">The second argument to check.</param>
		/// <param name="third">The third argument to check.</param>
		/// <param name="fourth">The fourth argument to check.</param>
		/// <param name="fifth">The fifth argument to check.</param>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		public static void Arguments<T1, T2, T3, T4, T5>(Func<T1> first, Func<T2> second, Func<T3> third, Func<T4> fourth,
			Func<T5> fifth)
			where T1 : class
			where T2 : class
			where T3 : class
			where T4 : class
			where T5 : class
		{
			if (null == first())
				throw Errors.ArgumentNull(first);

			if (null == second())
				throw Errors.ArgumentNull(second);

			if (null == third())
				throw Errors.ArgumentNull(third);

			if (null == fourth())
				throw Errors.ArgumentNull(fourth);

			if (null == fifth())
				throw Errors.ArgumentNull(fifth);
		}

		/// <summary>
		/// Throws proper exception if the provided string argument is null or empty. 
		/// </summary>
		/// <returns>Original string.</returns>
		/// <exception cref="ArgumentException">If the string argument is null or empty.</exception>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void ArgumentNotEmpty(Func<string> argumentReference)
		{
			var value = argumentReference();
			if (null == value)
				throw Errors.ArgumentNull(argumentReference);

			if (0 == value.Length)
				throw Errors.Argument(argumentReference, "String can't be empty");
		}


		/// <summary>
		/// Throws exception if the check does not pass.
		/// </summary>
		/// <param name="check">if set to <c>true</c> then check will pass.</param>
		/// <param name="name">The name of the assertion.</param>
		/// <exception cref="InvalidOperationException">If the assertion has failed.</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool check, [NotNull] string name)
		{
			if (!check)
			{
				throw Errors.InvalidOperation("Failed assertion '{0}'", name);
			}
		}

		/// <summary>
		/// Throws exception if the check does not pass.
		/// </summary>
		/// <param name="check">if set to <c>true</c> then check will pass.</param>
		/// <param name="message">The message.</param>
		/// <param name="arguments">The format arguments.</param>
		/// <exception cref="InvalidOperationException">If the assertion has failed.</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool check, [NotNull] string message, params object[] arguments)
		{
			if (!check)
			{
				throw Errors.InvalidOperation(message, arguments);
			}
		}

		/// <summary>
		/// Throws exception if the check does not pass.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the assertion has failed.</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool check)
		{
			if (!check)
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Throws <typeparamref name="TException"/> if the <paramref name="check"/>
		/// failes
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="check">Check that should be true.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">String arguments.</param>
		/// <exception cref="Exception">of <typeparamref name="TException"/> type</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		[StringFormatMethod("message")]
		public static void With<TException>(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool check, [NotNull] string message,
			params object[] args) where TException : Exception
		{
			if (!check)
			{
				var msg = args.Length == 0 ? message : string.Format(CultureInfo.InvariantCulture, message, args);
				throw (TException) Activator.CreateInstance(typeof (TException), msg);
			}
		}

		/// <summary> Throws exception if the argument fails the <paramref name="check"/> </summary>
		/// <param name="check">Throw exception if false.</param>
		/// <param name="paramName">Name of the param.</param>
		/// <param name="checkName">Name of the check.</param>
		/// <exception cref="ArgumentException">When the argument fails the check</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void Argument(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool check,
			[InvokerParameterName] string paramName, [NotNull] string checkName)
		{
			if (!check)
			{
				throw new ArgumentException(string.Format("Argument '{0}' has failed check '{1}'.", paramName, checkName), paramName);
			}
		}


		/// <summary>
		/// Throws proper exception if the class reference is null.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="value">Class reference to check.</param>
		/// <exception cref="InvalidOperationException">If class reference is null.</exception>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void NotNull<TValue>(Func<TValue> value) where TValue : class
		{
			if (value() == null)
			{
				throw Errors.InvalidOperation("'{0}' can not be null.", Reflect.VariableName(value));
			}
		}

		/// <summary>
		/// Throws proper exception if the class reference is null.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="value">Class reference to check.</param>
		/// <exception cref="InvalidOperationException">If class reference is null.</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void NotNull<TValue>(TValue value) where TValue : class
		{
			if (value == null)
			{
				throw Errors.InvalidOperation("Value of type '{0}' can not be null.", typeof(TValue).Name);
			}
		}

		/// <summary>
		/// Throws proper exception if the class reference is null.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="value">Class reference to check.</param>
		/// <param name="name">The name.</param>
		/// <exception cref="InvalidOperationException">If class reference is null.</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void NotNull<TValue>(TValue value, [NotNull] string name) where TValue : class
		{
			if (name == null) throw new ArgumentNullException("name");
			if (value == null)
			{
				throw Errors.InvalidOperation("Value '{1}' of type '{0}' can not be null.", typeof(TValue).Name, name);
			}
		}

		/// <summary>
		/// Throws proper exception if the provided class reference is null.
		/// Can be used for inline checks.
		/// </summary>
		/// <typeparam name="TValue">Class type</typeparam>
		/// <param name="value">Class reference to check.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Original reference.</returns>
		/// <exception cref="ArgumentNullException">If the class reference is null.</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		[Obsolete("Use Enforce.Argument() instead", true)]
		public static TValue ArgumentNotNull<TValue>(
			[AssertionCondition(AssertionConditionType.IS_NOT_NULL)] TValue value,
			[InvokerParameterName] string argumentName) where TValue : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(argumentName);
			}
			return value;
		}

		/// <summary>
		/// Runs the rules against single argument, using scope that fails on <see cref="Scope.WhenError"/>
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="argumentReference">The argument reference.</param>
		/// <param name="rules">The rules.</param>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		/// <exception cref="ArgumentException">When any rule fails</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void Argument<T>(Func<T> argumentReference, params Rule<T>[] rules)
		{
			using (var scope = ScopeFactory.ForEnforceArgument(argumentReference, Scope.WhenError))
			{
				scope.ValidateInScope(argumentReference(), rules);
			}
		}

		/// <summary>
		/// Runs the rules against single collection, using scope that fails on <see cref="Scope.WhenError"/>
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="items">The items to validate.</param>
		/// <param name="rules">The rules.</param>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		/// <exception cref="ArgumentException">When any rule fails</exception>
		[DebuggerNonUserCode]
		public static void Argument<T>(Func<IEnumerable<T>> items, params Rule<T>[] rules)
		{
			using (var scope = ScopeFactory.ForEnforceArgument(items, Scope.WhenError))
			{
				scope.ValidateInScope(items(), rules);
			}
		}


		/// <summary> Runs the rules against single argument, 
		/// using scope that fails on <see cref="Scope.WhenError"/>.
		///  </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="item">The item to validate.</param>
		/// <param name="rules">The rules.</param>
		/// <exception cref="RuleException">When check fails</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That<T>(
			[AssertionCondition(AssertionConditionType.IS_NOT_NULL)] T item,
			params Rule<T>[] rules)
		{
			using (var scope = Scope.ForEnforce(typeof (T).Name, Scope.WhenError))
			{
				scope.ValidateInScope(item, rules);
			}
		}

		/// <summary> Runs the rules against single collection, using 
		/// scope that fails on <see cref="Scope.WhenError"/>.</summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="items">The items to validate.</param>
		/// <param name="rules">The rules.</param>
		/// <exception cref="RuleException">When check fails</exception>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That<T>(
			[AssertionCondition(AssertionConditionType.IS_NOT_NULL)] IEnumerable<T> items,
			params Rule<T>[] rules)
		{
			using (var scope = Scope.ForEnforce(typeof (T).Name, Scope.WhenError))
			{
				scope.ValidateInScope(items, rules);
			}
		}


		/// <summary> Runs the rules against single item, using 
		/// scope that fails on <see cref="Scope.WhenError"/>.</summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="argumentReference">The item to validate.</param>
		/// <param name="rules">The rules.</param>
		/// <exception cref="RuleException">When check fails</exception>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That<T>(
			Func<T> argumentReference,
			params Rule<T>[] rules)
		{
			var name = Reflection.Reflect.VariableName(argumentReference);

			using (var scope = Scope.ForEnforce(name, Scope.WhenError))
			{
				scope.ValidateInScope(argumentReference(), rules);
			}
		}

		/// <summary> Runs the rules against collection, 
		/// using scope that fails on <see cref="Scope.WhenError"/> </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="collectionReference">The items to validate.</param>
		/// <param name="rules">The rules.</param>
		/// <exception cref="RuleException">When check fails</exception>
		/// <remarks>Silverlight 2.0 does not support fast resolution of variable names, yet</remarks>
		[DebuggerNonUserCode]
		[AssertionMethod]
		public static void That<T>(
			Func<IEnumerable<T>> collectionReference,
			params Rule<T>[] rules)
		{
			var name = Reflection.Reflect.VariableName(collectionReference);
			using (var scope = Scope.ForEnforce(name, Scope.WhenError))
			{
				scope.ValidateInScope(collectionReference(), rules);
			}
		}
	}
}