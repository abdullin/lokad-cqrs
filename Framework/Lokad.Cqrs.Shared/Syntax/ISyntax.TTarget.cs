#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.ComponentModel;

namespace Lokad
{
	/// <summary>
	/// Interface for implementing decoupled configuration extensions. It hides
	/// unnecessary members from the IntelliSense.
	/// </summary>
	/// <typeparam name="TTarget">syntax target</typeparam>
	public interface ISyntax<out TTarget>
	{
		/// <summary>
		/// Gets the underlying object.
		/// </summary>
		/// <value>The underlying object.</value>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		TTarget Target { get; }

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		string ToString();

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		bool Equals(object obj);

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		int GetHashCode();

		/// <summary>
		/// Gets the <see cref="T:System.Type"/> of the current instance.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Type"/> instance that represents the exact runtime type of the current instance.
		/// </returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		Type GetType();
	}
}