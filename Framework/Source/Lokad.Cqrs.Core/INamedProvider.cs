#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary>
	/// Shortcut interface for <see cref="IProvider{TKey,TValue}"/> that uses <see cref="string"/> as the key.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	[CLSCompliant(true)]
	public interface INamedProvider<TValue> : IProvider<string, TValue>
	{
	}
}