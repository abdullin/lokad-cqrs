#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion


namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicEntityReader<TView>
        //where TView : IAtomicEntity<TKey>
    {
        /// <summary>
        /// Gets the view with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="view">The view.</param>
        /// <returns>
        /// true, if it exists
        /// </returns>
        bool TryGet(object key, out TView view);
    }
}