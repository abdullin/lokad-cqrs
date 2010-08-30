#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Settings
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


		/// <summary>
		/// 	Creates new provider that contains only values filtered by the acceptor
		/// </summary>
		/// <param name="acceptor">The acceptor.</param>
		/// <returns>
		/// 	new settings provider that had been filtered
		/// </returns>
		ISettingsProvider Filtered(ISettingsKeyFilter acceptor);
	}
}