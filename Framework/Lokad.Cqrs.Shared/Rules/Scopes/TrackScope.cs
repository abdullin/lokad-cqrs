#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Rules
{
	/// <summary>
	/// <see cref="IScope"/> that merely keeps track of the worst level. 
	/// </summary>
	[Serializable]
	public sealed class TrackScope : IScope
	{
		RuleLevel _level;
		readonly Action<RuleLevel> _report = level => { };

		/// <summary>
		/// Initializes a new instance of the <see cref="TrackScope"/> class.
		/// </summary>
		public TrackScope()
		{
		}

		TrackScope(Action<RuleLevel> report)
		{
			_report = report;
		}

		void IDisposable.Dispose()
		{
			_report(_level);
		}

		IScope IScope.Create(string name)
		{
			return new TrackScope(level =>
				{
					if (level > _level)
						_level = level;
				});
		}

		void IScope.Write(RuleLevel level, string message)
		{
			if (level > _level)
				_level = level;
		}

		RuleLevel IScope.Level
		{
			get { return _level; }
		}
	}
}