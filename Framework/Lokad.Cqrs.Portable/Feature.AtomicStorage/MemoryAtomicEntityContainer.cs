using System;
using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicEntityContainer<TKey,TEntity> : IAtomicEntityReader<TKey,TEntity>, IAtomicEntityWriter<TKey,TEntity>
    {
        readonly IAtomicStorageStrategy _strategy;
        readonly string _entityPrefix;
        readonly ConcurrentDictionary<string, byte[]> _store;

        public MemoryAtomicEntityContainer(ConcurrentDictionary<string, byte[]> store, IAtomicStorageStrategy strategy)
        {
            _store = store;
            _strategy = strategy;
            _entityPrefix = _strategy.GetFolderForEntity(typeof(TEntity)) + ":";
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var name = _entityPrefix + _strategy.GetNameForEntity(typeof(TEntity), key);
            byte[] bytes;
            if(_store.TryGetValue(name, out bytes))
            {
               using (var mem = new MemoryStream(bytes))
               {
                   entity = _strategy.Deserialize<TEntity>(mem);
                   return true;
               }
            }
            entity = default(TEntity);
            return false;
        }


        public TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
        {
            var id = _entityPrefix + _strategy.GetNameForEntity(typeof (TEntity), key);
            var result = default(TEntity);
            _store.AddOrUpdate(id, s =>
                {
                    result = addFactory();
                    using (var memory = new MemoryStream())
                    {
                        _strategy.Serialize(result, memory);
                        return memory.ToArray();
                    }
                }, (s2, bytes) =>
                    {
                        TEntity entity;
                        using (var memory = new MemoryStream(bytes))
                        {
                            entity = _strategy.Deserialize<TEntity>(memory);
                        }
                        result = update(entity);
                        using (var memory = new MemoryStream())
                        {
                            _strategy.Serialize(result, memory);
                            return memory.ToArray();
                        }
                    });
            return result;
        }
     

        public bool TryDelete(TKey key)
        {
            var id = _entityPrefix + _strategy.GetNameForEntity(typeof(TEntity), key);
            byte[] bytes;
            return _store.TryRemove(id, out bytes);
        }
    }
}