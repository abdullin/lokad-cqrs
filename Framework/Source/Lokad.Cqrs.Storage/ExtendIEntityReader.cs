#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using Lokad.Quality;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Extends <see cref="IEntityReader"/>
	/// </summary>
	[UsedImplicitly]
	public static class ExtendIEntityReader
	{
		/// <summary>
		/// Retrieves the specified entity from the store, if it is found.
		/// Underlying storage could be event, cloud blob or whatever
		/// </summary>
		/// <typeparam name="T">type of the entity</typeparam>
		/// <param name="store">The store.</param>
		/// <param name="identity">The identity.</param>
		/// <returns>loaded entity (if found)</returns>
		public static Maybe<T> Read<T>(this IEntityReader store, object identity)
		{
			return store.Read(typeof (T), identity).Convert(o => (T) o);
		}
	}
}