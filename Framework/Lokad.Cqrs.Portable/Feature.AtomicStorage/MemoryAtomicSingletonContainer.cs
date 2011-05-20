#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicSingletonContainer<TEntity> :
        IAtomicSingletonReader<TEntity>, IAtomicSingletonWriter<TEntity>
    {
        readonly ConcurrentDictionary<string, byte[]> _store;
        readonly IAtomicStorageStrategy _strategy;
        readonly string _key;

        public MemoryAtomicSingletonContainer(ConcurrentDictionary<string,byte[]> store, IAtomicStorageStrategy strategy)
        {
            _store = store;
            _strategy = strategy;
            _key = _strategy.GetFolderForSingleton() + ":" + _strategy.GetNameForSingleton(typeof(TEntity));
        }

        public bool TryGet(out TEntity singleton)
        {
            byte[] bytes;
            if (_store.TryGetValue(_key, out bytes))
            {
                using (var mem = new MemoryStream(bytes))
                {
                    singleton = _strategy.Deserialize<TEntity>(mem);
                    return true;
                }
            }
            singleton = default(TEntity);
            return false;
        }

        public TEntity AddOrUpdate(Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
        {
            var result = default(TEntity);
            _store.AddOrUpdate(_key, s =>
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


       

        public bool TryDelete()
        {
            byte[] bytes;
            return _store.TryRemove(_key, out bytes);
        }
    }
}