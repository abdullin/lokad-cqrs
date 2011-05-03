#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.ComponentModel;
using System.Globalization;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Basic usability wrapper for the atomic storage operations, that does not enforce concurrency handling. 
    /// If you want to work with advanced functionality, either request specific interfaces from the container 
    /// or go through the advanced members on this instance. 
    /// </summary>
    /// <remarks>
    /// If you use as a stand-alone, make sure to call <see cref="Initialize"/> before proceeding.
    /// </remarks>
    public sealed class NuclearStorage 
    {
        public readonly IAtomicStorageFactory Factory;

        public NuclearStorage(IAtomicStorageFactory factory)
        {
            Factory = factory;
        }

        static string KeyToString(object key)
        {
            return Convert.ToString(key, CultureInfo.InvariantCulture);
        }

        public bool TryDeleteEntity<TEntity>(object key)
        {
            return Factory.GetEntityWriter<object,TEntity>().TryDelete(KeyToString(key));
        }

        public bool TryDeleteSingleton<TEntity>()
        {
            return Factory.GetSingletonWriter<TEntity>().TryDelete();
        }

        public TEntity UpdateOrThrowEntity<TEntity>(object key, Action<TEntity> update)
        {
            var id = KeyToString(key);
            return Factory.GetEntityWriter<object,TEntity>().UpdateOrThrow(id, update);
        }

  
        public TSingleton UpdateOrThrowSingleton<TSingleton>(Action<TSingleton> update) 
        {
            return Factory.GetSingletonWriter<TSingleton>().UpdateOrThrow(update);
        }

 

        public Maybe<TEntity> GetEntity<TEntity>(object key)
        {
            var id = KeyToString(key);
            return Factory.GetEntityReader<object,TEntity>().Get(id);
        }

        public bool TryGetEntity<TEntity>(object key, out TEntity entity)
        {
            var id = KeyToString(key);
            return Factory.GetEntityReader<object,TEntity>().TryGet(id, out entity);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, TEntity entity)
        {
            var id = KeyToString(key);
            return Factory.GetEntityWriter<object,TEntity>().AddOrUpdate(id, () => entity, source => entity);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            var id = KeyToString(key);
            return Factory.GetEntityWriter<object,TEntity>().AddOrUpdate(id, addFactory, update);
        }

        public TSingleton AddOrUpdateSingleton<TSingleton>(Func<TSingleton> addFactory, Action<TSingleton> update) 
        {
            return Factory.GetSingletonWriter<TSingleton>().AddOrUpdate(addFactory, update);
        }

        public TSingleton AddOrUpdateSingleton<TSingleton>(Action<TSingleton> update) where TSingleton : new()
        {
            return Factory.GetSingletonWriter<TSingleton>().AddOrUpdate(() => new TSingleton(), update);
        }

        public TSingleton GetOrNewSingleton<TSingleton>() where TSingleton : new()
        {
            return Factory.GetSingletonReader<TSingleton>().GetOrNew();
        }

        public Maybe<TSingleton> GetSingleton<TSingleton>()
        {
            return Factory.GetSingletonReader<TSingleton>().Get();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }


    }
}