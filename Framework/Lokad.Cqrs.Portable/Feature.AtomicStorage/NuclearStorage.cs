#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.ComponentModel;

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

        public bool TryDeleteEntity<TEntity>(object key)
        {
            return Factory.GetEntityWriter<object, TEntity>().TryDelete(key);
        }

        public bool TryDeleteSingleton<TEntity>()
        {
            return Factory.GetSingletonWriter<TEntity>().TryDelete();
        }

        public TEntity UpdateEntity<TEntity>(object key, Action<TEntity> update)
        {
            return Factory.GetEntityWriter<object, TEntity>().UpdateOrThrow(key, update);
        }


        public TSingleton UpdateSingletonOrThrow<TSingleton>(Action<TSingleton> update)
        {
            return Factory.GetSingletonWriter<TSingleton>().UpdateOrThrow(update);
        }


        public Optional<TEntity> GetEntity<TEntity>(object key)
        {
            return Factory.GetEntityReader<object, TEntity>().Get(key);
        }

        public bool TryGetEntity<TEntity>(object key, out TEntity entity)
        {
            return Factory.GetEntityReader<object, TEntity>().TryGet(key, out entity);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, TEntity entity)
        {
            return Factory.GetEntityWriter<object, TEntity>().AddOrUpdate(key, () => entity, source => entity);
        }

        public TEntity AddOrUpdateEntity<TEntity>(object key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            return Factory.GetEntityWriter<object, TEntity>().AddOrUpdate(key, addFactory, update);
        }

        public TEntity AddEntity<TEntity>(object key, TEntity newEntity)
        {
            return Factory.GetEntityWriter<object, TEntity>().Add(key, newEntity);
        }

        public TSingleton AddOrUpdateSingleton<TSingleton>(Func<TSingleton> addFactory, Action<TSingleton> update)
        {
            return Factory.GetSingletonWriter<TSingleton>().AddOrUpdate(addFactory, update);
        }

        public TSingleton AddOrUpdateSingleton<TSingleton>(Func<TSingleton> addFactory, Func<TSingleton,TSingleton> update)
        {
            return Factory.GetSingletonWriter<TSingleton>().AddOrUpdate(addFactory, update);
        }

        public TSingleton UpdateSingletonEnforcingNew<TSingleton>(Action<TSingleton> update) where TSingleton : new()
        {
            return Factory.GetSingletonWriter<TSingleton>().UpdateEnforcingNew(update);
        }

        public TSingleton GetSingletonOrNew<TSingleton>() where TSingleton : new()
        {
            return Factory.GetSingletonReader<TSingleton>().GetOrNew();
        }


        public Optional<TSingleton> GetSingleton<TSingleton>()
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