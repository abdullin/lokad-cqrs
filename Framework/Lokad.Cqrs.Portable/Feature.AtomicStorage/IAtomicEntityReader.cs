#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion


namespace Lokad.Cqrs.Feature.AtomicStorage
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