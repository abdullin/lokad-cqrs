using System;
using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicEntityContainer<TEntity> : IAtomicEntityReader<TEntity>, IAtomicEntityWriter<TEntity>
    {
        readonly ConcurrentDictionary<string,TEntity> _entities = new ConcurrentDictionary<string, TEntity>();

        public bool TryGet(string key, out TEntity entity)
        {
            return _entities.TryGetValue(key, out entity);
        }

        public TEntity Load(string key)
        {
            throw new NotImplementedException();
        }

        public TEntity AddOrUpdate(string key, Func<TEntity> addFactory, Func<TEntity, TEntity> update)
        {
            throw new NotImplementedException();
        }

        public TEntity AddOrUpdate(string key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            throw new NotImplementedException();
        }

        public TEntity AddOrUpdate(string key, TEntity newView, Action<TEntity> update)
        {
            throw new NotImplementedException();
        }

        public TEntity UpdateOrAdd(string key, Func<TEntity, TEntity> update, Func<TEntity> ifNone)
        {
            throw new NotImplementedException();
        }

        public TEntity UpdateOrAdd(string key, Action<TEntity> update, Func<TEntity> ifNone)
        {
            throw new NotImplementedException();
        }

        public TEntity UpdateOrThrow(string key, Action<TEntity> change)
        {
            throw new NotImplementedException();
        }

        public TEntity UpdateOrThrow(string key, Func<TEntity, TEntity> change)
        {
            throw new NotImplementedException();
        }

        public void Delete(string key)
        {
            throw new NotImplementedException();
        }
    }
}