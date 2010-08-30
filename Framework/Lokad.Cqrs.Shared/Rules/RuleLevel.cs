#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Rules
{
	/// <summary>
	/// Levels leveraged by the <see cref="Rule{T}"/> implementations
	/// </summary>
	public enum RuleLevel
	{
		/// <summary> Default value for the purposes of good citizenship </summary>
		None = 0,
		/// <summary> The rule raises a warning </summary>
		Warn,
		/// <summary> The rule raises an error </summary>
		Error
	}
}