#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicStorageFactory : IAtomicStorageFactory
    {
        readonly ConcurrentDictionary<string, byte[]> _store;
        readonly IAtomicStorageStrategy _strategy;

        public MemoryAtomicStorageFactory(ConcurrentDictionary<string,byte[]> store, IAtomicStorageStrategy strategy)
        {
            _store = store;
            _strategy = strategy;
        }

        public IAtomicEntityWriter<TKey,TEntity> GetEntityWriter<TKey,TEntity>()
        {
            return new MemoryAtomicEntityContainer<TKey, TEntity>(_store,_strategy);
        }


        public IAtomicEntityReader<TKey, TEntity> GetEntityReader<TKey, TEntity>()
        {
            return new MemoryAtomicEntityContainer<TKey, TEntity>(_store,_strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
            return new MemoryAtomicSingletonContainer<TSingleton>(_store,_strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
            return new MemoryAtomicSingletonContainer<TSingleton>(_store,_strategy);
        }

        public IEnumerable<string> Initialize()
        {
            return Enumerable.Empty<string>();
        }
    }
}