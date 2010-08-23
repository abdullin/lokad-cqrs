#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using Lokad.Diagnostics;
using Lokad.Quality;

namespace Lokad
{
	/// <summary>
	/// Extends logging syntax
	/// </summary>
	public static class ExtendISupportSyntaxForLogging
	{
		/// <summary>
		/// Registers the <see cref="TraceLog"/>
		/// </summary>
		/// <typeparam name="TModule">The type of the module.</typeparam>
		/// <param name="module">The module.</param>
		/// <returns>same module for the inlining</returns>
		[UsedImplicitly]
		public static TModule LogToTrace<TModule>(this TModule module)
			where TModule : ISupportSyntaxForLogging
		{
			module.RegisterLogProvider(TraceLog.Provider);
			return module;
		}

		/// <summary>
		/// Registers the <see cref="NullLog"/>
		/// </summary>
		/// <typeparam name="TModule">The type of the module.</typeparam>
		/// <param name="module">The module.</param>
		/// <returns>same module for the inlining</returns>
		[UsedImplicitly]
		public static TModule LogToNull<TModule>(this TModule module)
			where TModule : ISupportSyntaxForLogging
		{
			module.RegisterLogProvider(TraceLog.Provider);
			return module;
		}
	}

	
}