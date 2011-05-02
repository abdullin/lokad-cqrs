#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicStorageFactory : IAtomicStorageFactory
    {
        readonly MemoryAtomicStorageStrategy _strategy;

        public MemoryAtomicStorageFactory(MemoryAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
        }

        public IAtomicEntityWriter<TEntity> GetEntityWriter<TEntity>()
        {
            return new MemoryAtomicEntityContainer<TEntity>(_strategy);
        }


        public IAtomicEntityReader<TEntity> GetEntityReader<TEntity>()
        {
            return new MemoryAtomicEntityContainer<TEntity>(_strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
            return new MemoryAtomicSingletonContainer<TSingleton>(_strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
            return new MemoryAtomicSingletonContainer<TSingleton>(_strategy);
        }

        public void Initialize() {}
    }
}