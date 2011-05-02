#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Basic usability wrapper for the atomic storage operations. If you want to work with advanced functionality,
    /// either request specific interfaces from the container or go through the advanced members on this instance. 
    /// </summary>
    /// <remarks>
    /// If you use as a stand-alone, make sure to call <see cref="Initialize"/> before proceeding.
    /// </remarks>
    public sealed class AtomicSimplified
    {

        public readonly IAtomicStorageFactory Factory;

        public AtomicSimplified(IAtomicStorageFactory factory)
        {
            Factory = factory;
        }

        public bool TryDelete<TEntity>(object key)
        {
            return Factory.GetEntityWriter<TEntity>().TryDelete(KeyToString(key));
        }

        static string KeyToString(object key)
        {
            return Convert.ToString(key, CultureInfo.InvariantCulture);
        }

        public bool TryDeleteSingleton<TEntity>()
        {
            return Factory.GetSingletonWriter<TEntity>().TryDelete();
        }

        public TEntity Update<TEntity>(object key, Action<TEntity> update)
        {
            var id = KeyToString(key);
            return Factory.GetEntityWriter<TEntity>().UpdateOrThrow(id, update);
        }

        public TSingleton UpdateSingleton<TSingleton>(Action<TSingleton> update) 
        {
            return Factory.GetSingletonWriter<TSingleton>().UpdateOrThrow(update);
        }

        public Maybe<TEntity> Get<TEntity>(object key)
        {
            var id = KeyToString(key);
            return Factory.GetEntityReader<TEntity>().Get(id);
        }

        public TEntity Get<TEntity>(object key, TEntity defaultValue)
        {
            return Get<TEntity>(key).GetValue(defaultValue);
        }

        public void Save<TEntity>(object key, TEntity entity)
        {
            var id = KeyToString(key);
            Factory.GetEntityWriter<TEntity>().AddOrUpdate(id, () => entity, source => entity);
        }

        public void SaveSingleton<TSingleton>(TSingleton singleton)
        {
            Factory.GetSingletonWriter<TSingleton>()
                .UpdateOrAdd(s => singleton, () => singleton);

        }

        public TSingleton GetSingletonOrNew<TSingleton>() where TSingleton : new()
        {
            return Factory.GetSingletonReader<TSingleton>().Get().GetValue(() => new TSingleton());
        }

        public Maybe<TSingleton> GetSingleton<TSingleton>()
        {
            return Factory.GetSingletonReader<TSingleton>().Get();
        }
    }
}