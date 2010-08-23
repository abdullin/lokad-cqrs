#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Rules
{
	/// <summary>
	/// This scope is just like <see cref="SimpleScope"/>
	/// but it delays name resolution
	/// </summary>
	[Serializable]
	sealed class DelayedScope : IScope
	{
		public delegate void Messenger(Func<string> pathProvider, RuleLevel level, string message);

		readonly Func<string> _name;
		readonly Messenger _messenger;
		readonly Action<RuleLevel> _dispose = level => { };
		string _cachedName;

		RuleLevel _level;

		string GetName()
		{
			if (null == _cachedName)
			{
				_cachedName = _name();
			}
			return _cachedName;
		}

		public DelayedScope(Func<string> name, Messenger action)
		{
			_name = name;
			_messenger = action;
		}

		internal DelayedScope(Func<string> name, Messenger action, Action<RuleLevel> dispose)
		{
			_name = name;
			_messenger = action;
			_dispose = dispose;
		}

		void IDisposable.Dispose()
		{
			_dispose(_level);
		}

		IScope IScope.Create(string name)
		{
			return new DelayedScope(() => Scope.ComposePathInternal(GetName(), name), _messenger, level =>
				{
					if (level > _level)
						_level = level;
				});
		}

		void IScope.Write(RuleLevel level, string message)
		{
			if (level > _level)
				_level = level;

			_messenger(GetName, level, message);
		}

		RuleLevel IScope.Level
		{
			get { return _level; }
		}
	}
}