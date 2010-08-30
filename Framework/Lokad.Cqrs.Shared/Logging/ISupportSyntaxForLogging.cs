#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

namespace Lokad
{
	/// <summary>
	/// Syntax extensions for Logging configurations
	/// </summary>
	public interface ISupportSyntaxForLogging
	{
		/// <summary>
		/// Registers the specified log provider instance as singleton.
		/// </summary>
		/// <param name="provider">The provider.</param>
		void RegisterLogProvider(ILogProvider provider);
	}
}