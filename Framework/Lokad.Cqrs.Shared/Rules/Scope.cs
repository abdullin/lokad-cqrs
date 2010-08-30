#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Lokad.Quality;

namespace Lokad.Rules
{
	/// <summary>
	/// Helper class that invokes different scopes
	/// </summary>
	[UsedImplicitly]
	public static class Scope
	{
		/// <summary>
		/// Returns true if <paramref name="level"/>
		/// is <see cref="RuleLevel.Error"/> or higher
		/// </summary>
		/// <param name="level">The level to check.</param>
		/// <returns>true if the condition is met</returns>
		[NoCodeCoverage]
		public static bool WhenError(RuleLevel level)
		{
			return level >= RuleLevel.Error;
		}

		/// <summary>
		/// Returns true if <paramref name="level"/>
		/// is not <see cref="RuleLevel.None"/>
		/// </summary>
		/// <param name="level">The level.</param>
		/// <returns>true if the condition is met</returns>
		[NoCodeCoverage]
		public static bool WhenAny(RuleLevel level)
		{
			return level > RuleLevel.None;
		}

		/// <summary>
		/// Returns true if <paramref name="ruleLevel"/>
		/// is <see cref="RuleLevel.Warn"/> or higher
		/// </summary>
		/// <param name="ruleLevel">The rule level.</param>
		/// <returns>true if the condition is met</returns>
		[NoCodeCoverage]
		public static bool WhenWarn(RuleLevel ruleLevel)
		{
			return ruleLevel >= RuleLevel.Warn;
		}

		/// <summary>
		/// Returns true if <paramref name="ruleLevel"/>
		/// is <see cref="RuleLevel.None"/> or higher
		/// </summary>
		/// <param name="ruleLevel">The rule level.</param>
		/// <returns>true if the condition is met</returns>
		[NoCodeCoverage]
		public static bool WhenNone(RuleLevel ruleLevel)
		{
			return ruleLevel == RuleLevel.None;
		}


		/// <summary>
		/// Creates scope that runs full check and then fails with 
		/// <see cref="RuleException"/>, if the resulting 
		/// <see cref="RuleLevel"/>  matches the <paramref name="predicate"/>
		/// </summary>
		/// <param name="name">The name for the scope.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>new scope instance</returns>
		public static IScope ForValidation(string name, Predicate<RuleLevel> predicate)
		{
			return ScopeFactory.ForValidation(name, predicate);
		}

		/// <summary>
		/// Creates scope that throws <see cref="RuleException"/> as soon as
		/// first message matching <paramref name="predicate"/> is received.
		/// </summary>
		/// <param name="scopeName">Name of the scope.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>new scope instance</returns>
		public static IScope ForEnforce(string scopeName, Predicate<RuleLevel> predicate)
		{
			return ScopeFactory.ForEnforce(scopeName, predicate);
		}

		/// <summary>
		/// Creates scope that throws <see cref="ArgumentException"/> as soon as
		/// first message matching <paramref name="predicate"/> is received.
		/// </summary>
		/// <param name="scopeName">Name of the scope.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>new scope instance</returns>
		public static IScope ForEnforceArgument(string scopeName, Predicate<RuleLevel> predicate)
		{
			return ScopeFactory.ForEnforceArgument(scopeName, predicate);
		}


		/// <summary>  Determines whether the specified item has problems of 
		/// <see cref="RuleLevel.Error"/> or higher.  </summary>
		/// <typeparam name="TItem">type of the item to run rules against</typeparam>
		/// <param name="item">The item to run rules against.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns>
		/// 	<c>true</c> if the specified item is in error state; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsError<TItem>(TItem item, params Rule<TItem>[] rules)
		{
			using (IScope scope = new TrackScope())
			{
				scope.ValidateInScope(item, rules);
				return scope.IsError();
			}
		}

		/// <summary>  Determines whether the specified item does not have any problems </summary>
		/// <typeparam name="TItem">type of the item to run rules against</typeparam>
		/// <param name="item">The item to run rules against.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns>
		/// 	<c>true</c> if the specified item is in error state; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsValid<TItem>(TItem item, params Rule<TItem>[] rules)
		{
			using (IScope scope = new TrackScope())
			{
				scope.ValidateInScope(item, rules);
				return scope.IsNone();
			}
		}

		/// <summary>  Determines whether the specified item has problems of 
		/// <see cref="RuleLevel.Warn"/> or higher.  </summary>
		/// <typeparam name="TItem">type of the item to run rules against</typeparam>
		/// <param name="item">The item to run rules against.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns>
		/// 	<c>true</c> if the specified item is in warning state; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsWarn<TItem>(TItem item, params Rule<TItem>[] rules)
		{
			using (IScope scope = new TrackScope())
			{
				scope.ValidateInScope(item, rules);
				return scope.IsWarn();
			}
		}

		/// <summary> Collects all rule messages associated with the 
		/// specified <paramref name="item"/> </summary>
		/// <typeparam name="TItem">The type of the item to run the rules against.</typeparam>
		/// <param name="item">The item to run the rules against.</param>
		/// <param name="name">The name of the scope.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns>read-only collection of <see cref="RuleMessage"/></returns>
		public static RuleMessages GetMessages<TItem>(TItem item, string name, params Rule<TItem>[] rules)
		{
			if (name == null) throw new ArgumentNullException("name");
			return SimpleScope.GetMessages(name, scope => scope.ValidateInScope(item, rules));
		}

		/// <summary>
		/// Gets the messages created by action being executed against the scope.
		/// </summary>
		/// <param name="name">The name for the scope.</param>
		/// <param name="scopeAction">The scope action.</param>
		/// <returns>read-only collection of <see cref="RuleMessage"/></returns>
		public static RuleMessages GetMessages([NotNull] string name, [NotNull] Action<IScope> scopeAction)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (scopeAction == null) throw new ArgumentNullException("scopeAction");

			return SimpleScope.GetMessages(name, scopeAction);
		}

		/// <summary>
		/// Collects all rule messages associated with the
		/// specified <paramref name="itemReference"/>
		/// </summary>
		/// <typeparam name="TItem">The type of the item to run the rules against.</typeparam>
		/// <param name="itemReference">The item reference.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns>
		/// read-only collection of <see cref="RuleMessage"/>
		/// </returns>
		public static RuleMessages GetMessages<TItem>(Func<TItem> itemReference, params Rule<TItem>[] rules)
		{
			if (itemReference == null) throw new ArgumentNullException("itemReference");
			return ScopeFactory.GetMessages(itemReference, scope => scope.ValidateInScope(itemReference(), rules));
		}


		/// <summary>
		/// Collects all rule messages associated with the
		/// specified <paramref name="sequenceReference"/>
		/// </summary>
		/// <typeparam name="TItem">The type of the item to run the rules against.</typeparam>
		/// <param name="sequenceReference">The sequence reference.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns> read-only collection of <see cref="RuleMessage"/> </returns>
		public static RuleMessages GetMessagesForMany<TItem>(Func<IEnumerable<TItem>> sequenceReference,
			params Rule<TItem>[] rules)
		{
			if (sequenceReference == null) throw new ArgumentNullException("sequenceReference");
			return ScopeFactory.GetMessages(sequenceReference, scope => scope.ValidateInScope(sequenceReference(), rules));
		}


		/// <summary> Collects all rule messages associated with the 
		/// specified <paramref name="items"/> </summary>
		/// <typeparam name="TItem">The type of the item to run the rules against.</typeparam>
		/// <param name="items">The item to run the rules against.</param>
		/// <param name="name">The name of the scope.</param>
		/// <param name="rules">The rules to execute.</param>
		/// <returns>read-only collection of <see cref="RuleMessage"/></returns>
		public static RuleMessages GetMessagesForMany<TItem>(IEnumerable<TItem> items, string name, params Rule<TItem>[] rules)
		{
			if (name == null) throw new ArgumentNullException("name");
			return SimpleScope.GetMessages(name, scope => scope.ValidateInScope(items, rules));
		}


		/// <summary>
		/// Runs full validation scan of the specified item and throws error
		/// if the level is not <see cref="RuleLevel.None"/>
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="item">The item to validate.</param>
		/// <param name="rules">The rules.</param>
		/// <exception cref="RuleException">if any rules have failed</exception>
		[DebuggerNonUserCode]
		public static void Validate<T>(T item, params Rule<T>[] rules)
		{
			using (var scope = ScopeFactory.ForValidation(typeof (T).Name, WhenAny))
			{
				scope.ValidateInScope(item, rules);
			}
		}

		/// <summary>
		/// Runs full validation scan of the specified collection
		/// </summary>
		/// <typeparam name="T">type of the item to validate</typeparam>
		/// <param name="items">The collection to validate.</param>
		/// <param name="rules">The validators.</param>
		/// <exception cref="RuleException">if any rules have failed</exception>
		[DebuggerNonUserCode]
		public static void ValidateMany<T>(IEnumerable<T> items, params Rule<T>[] rules)
		{
			using (var scope = ScopeFactory.ForValidation(typeof (T).Name, WhenAny))
			{
				scope.ValidateInScope(items, rules);
			}
		}

		/// <summary>
		/// Rule path separator char
		/// </summary>
		[UsedImplicitly]
		public const char RulePathSeprator = '.';

		[NoCodeCoverage]
		internal static string ComposePathInternal(string prefix, string suffix)
		{
			// no checks here, since the call is coming from the trusted code.
			return prefix + RulePathSeprator + suffix;
		}

		/// <summary>
		/// Composes the path for the <see cref="Rule{T}"/>.
		/// </summary>
		/// <param name="prefix">The prefix.</param>
		/// <param name="suffix">The suffix.</param>
		/// <returns>composed path</returns>
		/// <exception cref="ArgumentException">if path parameters are null or empty</exception>
		[NoCodeCoverage]
		public static string ComposePath(string prefix, string suffix)
		{
			if (string.IsNullOrEmpty(prefix)) throw new ArgumentException("prefix");
			if (string.IsNullOrEmpty(suffix)) throw new ArgumentException("suffix");

			return ComposePathInternal(prefix, suffix);
		}

		/// <summary>
		/// Creates new scope for logging into the provided string builder
		/// </summary>
		/// <param name="scopeName">Name of the scope.</param>
		/// <param name="log">The builder to write to.</param>
		/// <returns>composed scope</returns>
		public static IScope ForLogging([NotNull] string scopeName, [NotNull] StringBuilder log)
		{
			if (log == null) throw new ArgumentNullException("log");
			if (string.IsNullOrEmpty(scopeName)) throw new ArgumentException("scopeName");

			var format = "{0} [{1}]: {2}" + Environment.NewLine;
			return new SimpleScope(scopeName, (path, level, message) => log.AppendFormat(format, path, level, message));
		}
	}
}