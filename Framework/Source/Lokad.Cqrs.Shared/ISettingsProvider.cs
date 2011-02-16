#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;

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

	/// <summary>
	/// Settings provider based on a simple dictionary
	/// </summary>
	public sealed class DictionarySettingsProvider : ISettingsProvider
	{
		readonly IDictionary<string, string> _dictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionarySettingsProvider"/> class.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		public DictionarySettingsProvider([NotNull] IDictionary<string, string> dictionary)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			_dictionary = dictionary;
		}

		Maybe<string> ISettingsProvider.GetValue([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return _dictionary.GetValue(name);
		}
	}
}