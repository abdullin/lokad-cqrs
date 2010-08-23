#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Settings
{
	/// <summary>
	/// 	Implements filtering interface for the
	/// 	<see cref="ISettingsProvider" />
	/// </summary>
	public interface ISettingsKeyFilter
	{
		/// <summary>
		/// 	Filters the path, either accepting it (and optionally altering)
		/// 	or by returning empty result
		/// </summary>
		/// <param name="keyPath">The key path.</param>
		/// <returns>filtered key path</returns>
		Maybe<string> Filter(string keyPath);
	}
}