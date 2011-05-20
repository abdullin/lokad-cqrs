#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicStorageFactory : IAtomicStorageFactory
    {
        public IAtomicEntityWriter<TKey, TEntity> GetEntityWriter<TKey, TEntity>()
        {
            return new AzureAtomicEntityWriter<TKey, TEntity>(_access, _strategy);
        }

        public IAtomicEntityReader<TKey, TEntity> GetEntityReader<TKey, TEntity>()
        {
            return new AzureAtomicEntityReader<TKey, TEntity>(_access, _strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
            return new AzureAtomicSingletonReader<TSingleton>(_access, _strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
            return new AzureAtomicSingletonWriter<TSingleton>(_access, _strategy);
        }

        readonly object _initializationLock = new object();
        bool _initialized;
        

        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        public IEnumerable<string> Initialize()
        {
            lock (_initializationLock)
            {
                if (_initialized)
                    return Enumerable.Empty<string>();
                var result = DoInitialize();

                _initialized = true;
                return result;
            }
        }

        string[] DoInitialize()
        {
            var folders = new HashSet<string>();

            var entityTypes = _strategy.GetEntityTypes().ToArray();
            var singletonTypes = _strategy.GetSingletonTypes().ToArray();

            if (entityTypes.Length == 0 && singletonTypes.Length == 0)
            {
                throw new InvalidOperationException(
                    string.Format("AzureAtomicStorage was configured, but without any entity or singleton definitions. Check info on your strategy: {0}", _strategy.GetType()));
            }

            foreach (var type in entityTypes)
            {
                var folder = _strategy.GetFolderForEntity(type);
                folders.Add(folder);
            }

            folders.Add(_strategy.GetFolderForSingleton());
            var client = _access.CreateBlobClient();


            var bag = new ConcurrentBag<string>();
            var all = folders
                .AsParallel()
                .Select(f =>
                    {
                        var container = client.GetContainerReference(f);
                        return Task.Factory.FromAsync(container.BeginCreateIfNotExist,
                            result =>
                                {
                                    var created = container.EndCreateIfNotExist(result);
                                    if (created)
                                    {
                                        bag.Add(f);
                                    }
                                    return created;
                                }, null);
                    })
                .ToArray();

            Task.WaitAll(all);

            return bag.ToArray();
        }

        readonly IAtomicStorageStrategy _strategy;
        readonly IAzureAccessConfiguration _access;


        public AzureAtomicStorageFactory(IAtomicStorageStrategy strategy, IAzureAccessConfiguration access)
        {
            _strategy = strategy;
            _access = access;
        }
    }
}