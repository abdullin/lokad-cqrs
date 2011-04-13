#region Copyright (c) 2009-2011 LOKAD SAS. All rights reserved.

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicState
{
	/// <summary>
	/// Strongly-typed view singleton writer
	/// </summary>
	/// <typeparam name="TView">The type of the view.</typeparam>
	public interface IAtomicSingletonWriter<TView> where TView : IAtomicSingleton
	{
		/// <summary>
		/// Adds new view singleton or updates an existing one.
		/// </summary>
		/// <param name="addFactory">The add factory.</param>
		/// <param name="updateFactory">The update factory (we are altering entity, hence the modifier and not Func).</param>
		void AddOrUpdate(Func<TView> addFactory, Action<TView> updateFactory);

		/// <summary>
		/// Deletes this view singleton.
		/// </summary>
		void Delete();
	}
}