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
	/// Simple helper extensions for <see cref="ICollection{T}"/>
	/// </summary>
	public static class ExtendICollection
	{
		/// <summary>
		/// Adds all items to the target collection
		/// </summary>
		/// <typeparam name="T">type of the item within the collection</typeparam>
		/// <param name="collection">The collection</param>
		/// <param name="items">items to add to the collection</param>
		/// <returns>same collection instance</returns>
		public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			if (items == null) throw new ArgumentNullException("items");

			foreach (var item in items)
			{
				collection.Add(item);
			}
			return collection;
		}

		/// <summary>
		/// Removes all items from the target collection
		/// </summary>
		/// <typeparam name="T">type of the item within the collection</typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="items">The items.</param>
		/// <returns>same collection instance</returns>
		public static ICollection<T> RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			if (items == null) throw new ArgumentNullException("items");

			foreach (var item in items)
			{
				collection.Remove(item);
			}

			return collection;
		}

		/// <summary>
		/// Shortcut to determine whether the specified <see cref="ICollection{T}"/> is empty.
		/// </summary>
		/// <typeparam name="T">items in the collection</typeparam>
		/// <param name="self">The collection.</param>
		/// <returns>
		/// 	<c>true</c> if the specified self is empty; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsEmpty<T>(this ICollection<T> self)
		{
			if (self == null) throw new ArgumentNullException("self");

			return self.Count == 0;
		}
	}
}