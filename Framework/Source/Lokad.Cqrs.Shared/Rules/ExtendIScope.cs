#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using Lokad.Quality;
using Lokad.Reflection;

namespace Lokad.Rules
{
	///<summary> /// <para>Extensions that encapsulate some repetitive tasks
	/// of setting scopes, and calling validation rules.</para>
	/// <para>Basically that's the class that links together scope 
	/// and validation logics.</para></summary>
	public static class ExtendIScope
	{
		/// <summary> Outputs formatted <see cref="RuleLevel.Error"/> 
		/// message into the  <paramref name="scope"/> using the 
		/// <see cref="CultureInfo.InvariantCulture"/> </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="message">The message (see <see cref="string.Format(string,object)"/>).</param>
		/// <param name="args">The arguments.</param>
		[StringFormatMethod("message")]
		public static void Error([NotNull] this IScope scope, [NotNull] string message, params object[] args)
		{
			scope.Write(RuleLevel.Error, string.Format(CultureInfo.InvariantCulture, message, args));
		}

		/// <summary> Outputs <see cref="RuleLevel.Error"/> 
		/// message into the  <paramref name="scope"/> </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="message">The message.</param>
		public static void Error([NotNull] this IScope scope, [NotNull] string message)
		{
			scope.Write(RuleLevel.Error, message);
		}

		/// <summary> Outputs formatted <see cref="RuleLevel.Warn"/> 
		/// message into the  <paramref name="scope"/> using the 
		/// <see cref="CultureInfo.InvariantCulture"/> </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="message">The message (see <see cref="string.Format(string,object)"/>).</param>
		/// <param name="args">The arguments.</param>
		[StringFormatMethod("message")]
		public static void Warn([NotNull] this IScope scope, [NotNull] string message, params object[] args)
		{
			scope.Write(RuleLevel.Warn, string.Format(CultureInfo.InvariantCulture, message, args));
		}

		/// <summary> Outputs <see cref="RuleLevel.Warn"/> 
		/// message into the  <paramref name="scope"/> </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="message">The message.</param>
		public static void Warn([NotNull] this IScope scope, [NotNull] string message)
		{
			scope.Write(RuleLevel.Warn, message);
		}


		/// <summary> Outputs formatted <see cref="RuleLevel.None"/> 
		/// message into the  <paramref name="scope"/> using the 
		/// <see cref="CultureInfo.InvariantCulture"/> </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="message">The message (see <see cref="string.Format(string,object)"/>).</param>
		/// <param name="args">The arguments.</param>
		[StringFormatMethod("message")]
		public static void Info([NotNull] this IScope scope, [NotNull] string message, params object[] args)
		{
			scope.Write(RuleLevel.None, string.Format(CultureInfo.InvariantCulture, message, args));
		}

		/// <summary> Outputs <see cref="RuleLevel.None"/> 
		/// message into the  <paramref name="scope"/> </summary>
		/// <param name="scope">The scope.</param>
		/// <param name="message">The message.</param>
		public static void Info([NotNull] this IScope scope, [NotNull] string message)
		{
			scope.Write(RuleLevel.None, message);
		}

		/// <summary> Determines whether the specified <paramref name="scope"/>
		/// is in the <see cref="RuleLevel.Error"/> state. </summary>
		/// <param name="scope">The scope.</param>
		/// <returns>
		/// 	<c>true</c> if the specified scope is in <see cref="RuleLevel.Error"/> 
		/// state; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsError([NotNull] this IScope scope)
		{
			return scope.Level >= RuleLevel.Error;
		}

		/// <summary> Determines whether the specified <paramref name="scope"/>
		/// is in the <see cref="RuleLevel.None"/> state. </summary>
		/// <param name="scope">The scope.</param>
		/// <returns>
		/// 	<c>true</c> if the specified scope is in <see cref="RuleLevel.None"/> 
		/// state; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNone([NotNull] this IScope scope)
		{
			return scope.Level == RuleLevel.None;
		}


		/// <summary> Determines whether the specified <paramref name="scope"/>
		/// is in the <see cref="RuleLevel.Warn"/> state. </summary>
		/// <param name="scope">The scope.</param>
		/// <returns>
		/// 	<c>true</c> if the specified scope is in <see cref="RuleLevel.Warn"/> 
		/// state; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsWarn([NotNull] this IScope scope)
		{
			return scope.Level >= RuleLevel.Warn;
		}


		/// <summary>
		/// Validates some member using the <paramref name="scopeProvider"/>.
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="scopeProvider">The scope provider.</param>
		/// <param name="item">The item to validate.</param>
		/// <param name="name">The name of the variable that holds item to validate.</param>
		/// <param name="rules">The rules to run.</param>
		public static void Validate<T>(this INamedProvider<IScope> scopeProvider, T item, string name, params Rule<T>[] rules)
		{
			if (scopeProvider == null) throw new ArgumentNullException("scopeProvider");
			if (string.IsNullOrEmpty(name)) throw Errors.ArgumentNullOrEmpty("name");
			// item can be null, it will be checked by the validation

			using (var scope = scopeProvider.Get(name))
			{
				scope.CheckObject(item, rules);
			}
		}


		/// <summary> Validates some member using the provided <paramref name="parentScope"/>.
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="parentScope">The parent scope.</param>
		/// <param name="item">The item to validate.</param>
		/// <param name="name">The name of the variable that holds item to validate.</param>
		/// <param name="rules">The rules to run.</param>
		public static void Validate<T>(this IScope parentScope,
			[NotNull] T item,
			[NotNull] string name, params Rule<T>[] rules)
		{
			if (parentScope == null) throw new ArgumentNullException("parentScope");
			if (string.IsNullOrEmpty(name)) throw Errors.ArgumentNullOrEmpty("name");
			// item can be null, it will be checked by the validation

			using (var scope = parentScope.Create(name))
			{
				scope.CheckObject(item, rules);
			}
		}


		/// <summary>
		/// Validates some member using the provided <paramref name="parentScope"/>.
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="parentScope">The parent scope.</param>
		/// <param name="property">The property reference.</param>
		/// <param name="rules">The rules to run.</param>
		public static void Validate<T>(this IScope parentScope,
			[NotNull] Func<T> property, params Rule<T>[] rules)
		{
			if (parentScope == null) throw new ArgumentNullException("parentScope");
			if (property == null) throw new ArgumentNullException("reference");
			// item can be null, it will be checked by the validation

			var name = Reflect.MemberName(property);
			using (var scope = parentScope.Create(name))
			{
				scope.CheckObject(property(), rules);
			}
		}


		/// <summary>
		/// Validates some <see cref="IEnumerable{T}"/> member using the provided <paramref name="parentScope"/>.
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="parentScope">The parent scope.</param>
		/// <param name="propertyReference">Reference to collection property to validate.</param>
		/// <param name="rules">The rules to run.</param>
		public static void ValidateMany<T>(this IScope parentScope, Func<IEnumerable<T>> propertyReference,
			params Rule<T>[] rules)
		{
			if (parentScope == null) throw new ArgumentNullException("parentScope");
			if (propertyReference == null) throw new ArgumentNullException("propertyReference");

			// item can be null, it will be checked by the validation
			var name = Reflect.MemberName(propertyReference);
			using (var scope = parentScope.Create(name))
			{
				scope.ValidateInScope(propertyReference(), rules);
			}
		}

		/// <summary>
		/// Validates some <see cref="IEnumerable{T}"/> member using the provided <paramref name="parentScope"/>.
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="parentScope">The parent scope.</param>
		/// <param name="items">The collection to validate.</param>
		/// <param name="name">The name of the variable that holds item to validate.</param>
		/// <param name="rules">The rules to run.</param>
		public static void ValidateMany<T>(this IScope parentScope, IEnumerable<T> items, string name, params Rule<T>[] rules)
		{
			if (parentScope == null) throw new ArgumentNullException("parentScope");
			if (string.IsNullOrEmpty(name)) throw Errors.ArgumentNullOrEmpty("name");
			// item can be null, it will be checked by the validation

			using (var scope = parentScope.Create(name))
			{
				scope.ValidateInScope(items, rules);
			}
		}

		/// <summary>
		/// Validates some <see cref="IEnumerable{T}"/> member using the provided <paramref name="parentScope"/>.
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="parentScope">The parent scope.</param>
		/// <param name="items">The collection to validate.</param>
		/// <param name="name">The name of the variable that holds item to validate.</param>
		/// <param name="limit">The limit (if collection is bigger, then validation will not continue).</param>
		/// <param name="rules">The rules to run.</param>
		public static void ValidateMany<T>(this IScope parentScope, ICollection<T> items, string name, int limit,
			params Rule<T>[] rules)
		{
			if (parentScope == null) throw new ArgumentNullException("parentScope");
			if (string.IsNullOrEmpty(name)) throw Errors.ArgumentNullOrEmpty("name");
			// item can be null, it will be checked by the validation

			using (var scope = parentScope.Create(name))
			{
				scope.ValidateInScope(items, rules);
			}
		}

		/// <summary>
		/// Runs the specified rules against the provided object in the current scope
		/// </summary>
		/// <typeparam name="T">type of the object being validated</typeparam>
		/// <param name="scope">The scope.</param>
		/// <param name="item">The item to validate.</param>
		/// <param name="rules">The rules to execute.</param>
		public static void ValidateInScope<T>(this IScope scope, T item, params Rule<T>[] rules)
		{
			if (scope == null) throw new ArgumentNullException("scope");
			// item can be null, it will be checked by the validation

			CheckObject(scope, item, rules);
		}


		static void CheckObject<T>(this IScope self, T item, params Rule<T>[] rules)
		{
			// [abdullin]: I know
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (item == null)
				// ReSharper restore CompareNonConstrainedGenericWithNull
			{
				self.Error(RuleResources.Object_X_cant_be_null, typeof (T).Name);
			}
			else
			{
				foreach (var rule in rules)
				{
					rule(item, self);
				}
			}
		}


		/// <summary>
		/// Runs validation rules against some some <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">type of the items being validated</typeparam>
		/// <param name="parentScope">The parent scope.</param>
		/// <param name="items">Collection of the items to validate</param>
		/// <param name="rules">The rules to run.</param>
		public static void ValidateInScope<T>(this IScope parentScope, IEnumerable<T> items, params Rule<T>[] rules)
		{
			if (parentScope == null) throw new ArgumentNullException("parentScope");

			if (items == null)
			{
				parentScope.Error(RuleResources.Collection_X_cant_be_null, typeof (T).Name);
			}
			else
			{
				int i = 0;

				foreach (var item in items)
				{
					using (var scope = parentScope.Create("[" + i + "]"))
					{
						scope.CheckObject(item, rules);
					}
					i += 1;
				}
			}
		}


		/// <summary>
		/// Creates a wrapper that lowers the importance of messages
		/// being passed to the specified <paramref name="scope"/>.
		/// </summary>
		/// <param name="scope">The scope to wrap.</param>
		/// <returns>new instance of the wrapper</returns>
		public static IScope Lower(this IScope scope)
		{
			return new ModifierScope(scope, ModifierScope.Lower);
		}

		/// <summary>
		/// Creates a wrapper that boosts the <see cref="RuleLevel"/> of
		/// messages being passed to the specified<paramref name="scope"/>.
		/// </summary>
		/// <param name="scope">The scope to wrap.</param>
		/// <returns>new instance of the wrapper</returns>
		public static IScope Raise(this IScope scope)
		{
			return new ModifierScope(scope, ModifierScope.Raise);
		}
	}
}