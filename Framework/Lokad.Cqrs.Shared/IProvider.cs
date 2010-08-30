#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary>
	/// Interface that abstracts away providers
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <remarks>
	/// things like IDataCache (from the Database layers) or IResolver (from the IoC layers) 
	/// are just samples of this interface
	/// </remarks>
	[CLSCompliant(true)]
	public interface IProvider<TKey, TValue>
	{
		/// <summary>
		/// Retrieves <typeparamref name="TValue"/> given the
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="ResolutionException">when the key can not be resolved</exception>
		TValue Get(TKey key);
	}
}