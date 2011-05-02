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

        public TEntity AddOrUpdate(Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
        {
            return (TEntity) _singletons.AddOrUpdate(_type, t => addFactory(), (type, o) => update((TEntity) o));
        }

        public TEntity AddOrUpdate(Func<TEntity> addFactory, Action<TEntity> update, AddOrUpdateHint hint)
        {
            return AddOrUpdate(addFactory, entity =>
                {
                    update(entity);
                    return entity;
                }, hint);
        }

        public TEntity UpdateOrThrow(Action<TEntity> update)
        {
            return AddOrUpdate(() => { throw new InvalidOperationException("Item expected to exist"); }, update, AddOrUpdateHint.ProbablyExists);
        }

        public TEntity UpdateOrThrow(Func<TEntity, TEntity> update)
        {
            return AddOrUpdate(() => { throw new InvalidOperationException("Item expected to exist"); }, update, AddOrUpdateHint.ProbablyExists);
        }

        public bool TryDelete()
        {
            object value;
            return _singletons.TryRemove(_type, out value);
        }
    }
}