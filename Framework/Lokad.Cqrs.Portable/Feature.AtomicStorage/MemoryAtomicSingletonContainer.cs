#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicSingletonContainer<TEntity> :
        IAtomicSingletonReader<TEntity>, IAtomicSingletonWriter<TEntity>
    {
        readonly ConcurrentDictionary<Type, object> _singletons;
        readonly Type _type = typeof (TEntity);

        public MemoryAtomicSingletonContainer(MemoryAtomicStorageStrategy strategy)
        {
            _singletons = strategy.GetSingletonContainer();
        }

        public bool TryGet(out TEntity singleton)
        {
            object value;
            if (_singletons.TryGetValue(_type, out value))
            {
                singleton = (TEntity) value;
                return true;
            }
            singleton = default(TEntity);
            return false;
        }

        public TEntity AddOrUpdate(Func<TEntity> addFactory, Func<TEntity, TEntity> update)
        {
            return (TEntity) _singletons.AddOrUpdate(_type, addFactory, (type, o) => update((TEntity) o));
        }

        public TEntity AddOrUpdate(Func<TEntity> addFactory, Action<TEntity> update)
        {
            return AddOrUpdate(addFactory, entity =>
                {
                    update(entity);
                    return entity;
                });
        }

        public TEntity UpdateOrAdd(Func<TEntity, TEntity> update, Func<TEntity> ifNone)
        {
            // no difference
            return AddOrUpdate(ifNone, update);
        }

        public TEntity UpdateOrAdd(Action<TEntity> update, Func<TEntity> ifNone)
        {
            // no difference
            return AddOrUpdate(ifNone, update);
        }

        public TEntity UpdateOrThrow(Action<TEntity> update)
        {
            return UpdateOrAdd(update, () => { throw new InvalidOperationException("Item expected to exist"); });
        }

        public TEntity UpdateOrThrow(Func<TEntity, TEntity> update)
        {
            return UpdateOrAdd(update, () => { throw new InvalidOperationException("Item expected to exist"); });
        }

        public bool TryDelete()
        {
            object value;
            return _singletons.TryRemove(_type, out value);
        }
    }
}