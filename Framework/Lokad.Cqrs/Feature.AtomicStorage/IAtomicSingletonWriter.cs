#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
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