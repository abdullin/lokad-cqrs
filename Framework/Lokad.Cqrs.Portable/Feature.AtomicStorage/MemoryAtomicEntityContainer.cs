using System;
using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicEntityContainer<TKey,TEntity> : IAtomicEntityReader<TKey,TEntity>, IAtomicEntityWriter<TKey,TEntity>
    {
        readonly ConcurrentDictionary<object,TEntity> _entities;

        public MemoryAtomicEntityContainer(MemoryAtomicStorageStrategy strategy)
        {
            _entities = strategy.GetEntityContainer<TEntity>();
        }
        public bool TryGet(TKey key, out TEntity entity)
        {
            return _entities.TryGetValue(key, out entity);
        }


        public TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
        {
            return _entities.AddOrUpdate(key, s => addFactory(), (s1, entity) => update(entity));
        }
     

        public bool TryDelete(TKey key)
        {
            TEntity value;
            return _entities.TryRemove(key, out value);
        }
    }
}