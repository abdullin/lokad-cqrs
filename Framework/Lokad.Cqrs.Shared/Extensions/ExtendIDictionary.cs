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
	/// Extensions for <see cref="IDictionary{TKey,TValue}"/>
	/// </summary>
	public static class ExtendIDictionary
	{
		/// <summary>
		/// Wraps the dictionary with the read-only provider instance
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="self">The dictionary.</param>
		/// <returns>provider instance that wraps the dictionary</returns>
		public static IProvider<TKey, TValue> AsProvider<TKey, TValue>(this IDictionary<TKey, TValue> self)
		{
			if (self == null) throw new ArgumentNullException("self");

			return new Provider<TKey, TValue>(key => self[key]);
		}

		/// <summary>
		/// Wraps the provider with the read-only provider instance
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="self">The dictionary.</param>
		/// <returns>provider instance that wraps the dictionary</returns>
		public static INamedProvider<TValue> AsProvider<TValue>(this IDictionary<string, TValue> self)
		{
			if (self == null) throw new ArgumentNullException("self");

			return new NamedProvider<TValue>(key => self[key]);
		}

		/// <summary>
		/// Returns <paramref name="defaultValue"/> if the given <paramref name="key"/>
		/// is not present within the dictionary
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="self">The dictionary.</param>
		/// <param name="key">The key to look for.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>value matching <paramref name="key"/> or <paramref name="defaultValue"/> if none is found</returns>
		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue defaultValue)
		{
			TValue value;
			if (self.TryGetValue(key, out value))
			{
				return value;
			}
			return defaultValue;
		}


		/// <summary>
		/// Gets the value from the <paramref name="dictionary"/> in form of the <see cref="Maybe{T}"/>.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <returns>value from the dictionary</returns>
		public static Maybe<TValue> GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue value;
			if (dictionary.TryGetValue(key, out value))
			{
				return value;
			}
			return Maybe<TValue>.Empty;
		}
	}
}