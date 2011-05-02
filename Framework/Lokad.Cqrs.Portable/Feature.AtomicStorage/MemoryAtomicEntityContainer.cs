using System;
using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicEntityContainer<TEntity> : IAtomicEntityReader<TEntity>, IAtomicEntityWriter<TEntity>
    {
        readonly ConcurrentDictionary<string,TEntity> _entities;

        public MemoryAtomicEntityContainer(MemoryAtomicStorageStrategy strategy)
        {
            _entities = strategy.GetEntityContainer<TEntity>();
        }
        public bool TryGet(string key, out TEntity entity)
        {
            return _entities.TryGetValue(key, out entity);
        }


        public TEntity AddOrUpdate(string key, Func<TEntity> addFactory, Func<TEntity, TEntity> update)
        {
            return _entities.AddOrUpdate(key, s => addFactory(), (s1, entity) => update(entity));
        }
       

        public TEntity UpdateOrAdd(string key, Func<TEntity, TEntity> update, Func<TEntity> ifNone)
        {
            return _entities.AddOrUpdate(key, s => ifNone(), (s1, entity) => update(entity));
        }

     

        public bool TryDelete(string key)
        {
            TEntity value;
            return _entities.TryRemove(key, out value);
        }
    }
}