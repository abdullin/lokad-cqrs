#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicStorageStrategy 
    {
        readonly ConcurrentDictionary<Type, object> _singletons = new ConcurrentDictionary<Type, object>();
        readonly ConcurrentDictionary<Type, object> _entities = new ConcurrentDictionary<Type, object>();
        
        public ConcurrentDictionary<Type, object> GetSingletonContainer()
        {
            return _singletons;
        }

        public ConcurrentDictionary<object,TEntity> GetEntityContainer<TEntity>()
        {
            return
                (ConcurrentDictionary<object, TEntity>)
                    _entities.GetOrAdd(typeof(TEntity), t => new ConcurrentDictionary<object, TEntity>());
        }
    }
}