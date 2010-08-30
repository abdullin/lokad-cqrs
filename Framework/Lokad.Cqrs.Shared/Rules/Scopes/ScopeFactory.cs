#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Reflection;

namespace Lokad.Rules
{
	static class ScopeFactory
	{
		public static IScope ForEnforceArgument(string name, Predicate<RuleLevel> throwIf)
		{
			return new SimpleScope(name, (path, level, message) =>
				{
					if (throwIf(level))
						throw new ArgumentException(message, path);
				});
		}

		public static IScope ForEnforceArgument<T>(Func<T> argumentReference, Predicate<RuleLevel> predicate)
		{
			return new DelayedScope(
				() => Reflect.VariableName(argumentReference),
				(func, level, s) =>
					{
						if (predicate(level))
							throw new ArgumentException(s, func());
					}
				);
		}

		public static IScope ForEnforce(string name, Predicate<RuleLevel> throwIf)
		{
			return new SimpleScope(name, (path, level, message) =>
				{
					if (throwIf(level))
						throw new RuleException(message, path);
				});
		}

		public static IScope ForEnforce<T>(Func<T> reference, Predicate<RuleLevel> predicate)
		{
			return new DelayedScope(
				() => Reflect.VariableName(reference),
				(func, level, s) =>
					{
						if (predicate(level))
							throw new RuleException(s, func());
					}
				);
		}

		public static IScope ForMessages(string name, Action<RuleMessages> action)
		{
			var messages = new List<RuleMessage>();
			return new SimpleScope(name,
				(path, level, message) => messages.Add(new RuleMessage(path, level, message)),
				level => action(new RuleMessages(messages, level)));
		}

		public static IScope ForMessages<T>(Func<T> reference, Action<RuleMessages> action)
		{
			var messages = new List<RuleMessage>();
			return new DelayedScope(
				() => Reflect.VariableName(reference),
				(func, level, message) => messages.Add(new RuleMessage(func(), level, message)),
				level => action(new RuleMessages(messages, level)));
		}

		public static IScope ForValidation(string name, Predicate<RuleLevel> throwIf)
		{
			return ForMessages(name, messages =>
				{
					if (throwIf(messages.Level))
						throw new RuleException(messages);
				});
		}

		public static RuleMessages GetMessages<T>(Func<T> reference, Action<IScope> action)
		{
			var messages = RuleMessages.Empty;
			using (var scope = ForMessages(reference, m => messages = m))
			{
				action(scope);
			}
			return messages;
		}
	}
}