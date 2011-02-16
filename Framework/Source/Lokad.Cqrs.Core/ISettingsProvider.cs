#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad
{
	/// <summary>
	/// 	Simple settings reader
	/// </summary>
	public interface ISettingsProvider
	{
		/// <summary>
		/// 	Gets the value, using the given key name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>
		/// 	value for the specified key name, or empty result
		/// </returns>
		Maybe<string> GetValue(string name);
	}
}