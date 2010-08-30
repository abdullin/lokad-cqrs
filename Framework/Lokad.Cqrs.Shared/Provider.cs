#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using Lokad.Quality;

namespace Lokad
{
	/// <summary>
	/// This class provides short-cut for creating providers
	/// out of lambda expressions.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	[Serializable]
	public class Provider<TKey, TValue> : IProvider<TKey, TValue>
	{
		readonly Func<TKey, TValue> _resolver;

		/// <summary>
		/// Initializes a new instance of the <see cref="Provider{TKey, TValue}"/> class.
		/// </summary>
		/// <param name="resolver">The resolver.</param>
		/// <exception cref="ArgumentNullException">When 
		/// <paramref name="resolver"/> is null</exception>
		public Provider(Func<TKey, TValue> resolver)
		{
			if (resolver == null) throw new ArgumentNullException("resolver");

			_resolver = resolver;
		}

		/// <summary>
		/// Retrieves <typeparamref name="TValue"/> given the
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="ResolutionException">when the key is invalid for
		/// the provider</exception>
		public TValue Get(TKey key)
		{
			try
			{
				return _resolver(key);
			}
			catch (Exception ex)
			{
				throw Errors.Resolution(typeof (TValue), key, ex);
			}
		}
	}

	/// <summary>
	/// Helper class that simplifies creation of <see cref="Provider{TKey,TValue}"/>
	/// </summary>
	/// <typeparam name="TKey">type of the Key items</typeparam>
	[NoCodeCoverage]
	public static class Provider<TKey>
	{
		/// <summary>
		/// Creates the provider, letting compiler to figure out
		/// the value type. This allows to use anonymous types locally as well
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="func">The function that is the provider.</param>
		/// <returns>new provider instance</returns>
		public static IProvider<TKey, TValue> For<TValue>(Func<TKey, TValue> func)
		{
			return new Provider<TKey, TValue>(func);
		}
	}
}