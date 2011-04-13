#region Copyright (c) 2009-2011 LOKAD SAS. All rights reserved.

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.AtomicState
{
	public interface IAtomicEntityReader<in TKey, TView>
		where TView : IAtomicEntity<TKey>
	{
		/// <summary>
		/// Gets the view with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>view, if it exists</returns>
		Maybe<TView> Get(TKey key);

		/// <summary>
		/// Gets the view, assuming it exists and throwing an exception overwise
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>view</returns>
		TView Load(TKey key);
	}
}