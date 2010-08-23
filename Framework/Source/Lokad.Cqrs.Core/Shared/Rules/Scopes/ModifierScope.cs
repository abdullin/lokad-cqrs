#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Rules
{
	/// <summary>
	/// Simple <see cref="IScope"/> implementation that allows to 
	/// modify the behavior of the underlying scopes
	/// </summary>
	[Serializable]
	sealed class ModifierScope : IScope
	{
		public delegate void Modifier(IScope scope, RuleLevel ruleLevel, string message);

		readonly Modifier _modifier;
		readonly IScope _inner;

		public ModifierScope(IScope inner, Modifier modifier)
		{
			_modifier = modifier;
			_inner = inner;
		}

		void IDisposable.Dispose()
		{
			//_inner.Dispose();
		}

		IScope IScope.Create(string name)
		{
			return new ModifierScope(_inner.Create(name), _modifier);
		}

		void IScope.Write(RuleLevel level, string message)
		{
			_modifier(_inner, level, message);
		}

		RuleLevel IScope.Level
		{
			get { return _inner.Level; }
		}

		public static void Lower(IScope scope, RuleLevel ruleLevel, string message)
		{
			switch (ruleLevel)
			{
				case RuleLevel.Warn:
					scope.Write(RuleLevel.None, message);
					break;
				case RuleLevel.None:
					break;
				case RuleLevel.Error:
					scope.Write(RuleLevel.Warn, message);
					break;
				default:
					throw new ArgumentOutOfRangeException("level");
			}
		}

		public static void Raise(IScope scope, RuleLevel ruleLevel, string message)
		{
			switch (ruleLevel)
			{
				case RuleLevel.None:
					scope.Write(RuleLevel.Warn, message);
					break;
				case RuleLevel.Warn:
				case RuleLevel.Error:
					scope.Write(RuleLevel.Error, message);
					break;
				default:
					throw new ArgumentOutOfRangeException("level");
			}
		}
	}
}