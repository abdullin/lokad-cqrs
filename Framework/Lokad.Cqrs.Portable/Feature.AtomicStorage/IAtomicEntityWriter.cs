#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// View writer interface, used by the event handlers
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    public interface IAtomicEntityWriter<TEntity> //where TEntity : IAtomicEntity<TKey>
    {
        TEntity AddOrUpdate(string key, Func<TEntity> addFactory, Func<TEntity, TEntity> update);
        TEntity AddOrUpdate(string key, Func<TEntity> addFactory, Action<TEntity> update);
        TEntity AddOrUpdate(string key, TEntity newView, Action<TEntity> update);

        TEntity UpdateOrAdd(string key, Func<TEntity, TEntity> update, Func<TEntity> ifNone);
        TEntity UpdateOrAdd(string key, Action<TEntity> update, Func<TEntity> ifNone);


        TEntity UpdateOrThrow(string key, Action<TEntity> change);
        TEntity UpdateOrThrow(string key, Func<TEntity,TEntity> change);

        void Delete(string key);
    }
}