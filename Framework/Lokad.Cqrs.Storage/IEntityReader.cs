#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Handles read-side operations for the entity storage
	/// </summary>
	public interface IEntityReader
	{
		/// <summary>
		/// Retrieves the specified entity from the store, if it is found.
		/// Underlying storage could be event, cloud blob or whatever
		/// </summary>
		/// <param name="type">The type of the state (needed to deserialize).</param>
		/// <param name="identity">The identity.</param>
		/// <returns>loaded entity (if found)</returns>
		Maybe<object> Read(Type type, object identity);
	}
}