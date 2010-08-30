#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Rules
{
	/// <summary>
	/// Concept from the xLim2. That's simple nesting logger that is used by
	/// the validation rules. 
	/// </summary>
	/// <remarks>
	/// <para>It has logical the extensibility (not implemented, because there
	/// does not seem to be any need) for maintaining the error level in
	/// attached and detached scopes. Warnings, Fatals or Info messages
	/// could be added here (full ILogScope if needed).</para>
	/// <para>  Same extensibility
	/// could be turned on for capturing detailed validation info on complex
	/// long-running validation scenarios (you'd hate to debug these). </para>
	/// <para> Note, that in order to maintain .NET 2.0 compatibility,
	/// is is recommended to use interface-declared methods instead of the
	/// extensions (or use some extension weaver).</para>
	/// </remarks>
	public interface IScope : IDisposable
	{
		/// <summary>
		/// Creates the nested scope with the specified name.
		/// </summary>
		/// <param name="name">New name for the nested scope.</param>
		/// <returns>Nested (and linked) scope instance</returns>
		IScope Create(string name);

		/// <summary>
		/// Writes <paramref name="message"/> with the specified
		/// <paramref name="level"/> to the <see cref="IScope"/>
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="message">The message.</param>
		void Write(RuleLevel level, string message);

		/// <summary>
		/// Gets the current <see cref="RuleLevel"/> of this scope
		/// </summary>
		/// <value>The level.</value>
		RuleLevel Level { get; }

		// used for complex multi-step integration scenarios 
		// IScope CreateDetached(string name)
	}


}