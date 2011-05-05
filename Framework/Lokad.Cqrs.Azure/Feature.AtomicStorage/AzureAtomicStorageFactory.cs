#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicStorageFactory : IAtomicStorageFactory
    {
        public IAtomicEntityWriter<TKey, TEntity> GetEntityWriter<TKey, TEntity>()
        {
            return new AzureAtomicEntityWriter<TKey, TEntity>(_client, _strategy);
        }

        public IAtomicEntityReader<TKey, TEntity> GetEntityReader<TKey, TEntity>()
        {
            return new AzureAtomicEntityReader<TKey, TEntity>(_client, _strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
            return new AzureAtomicSingletonReader<TSingleton>(_client, _strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
            return new AzureAtomicSingletonWriter<TSingleton>(_client, _strategy);
        }

        readonly object _initializationLock = new object();
        bool _initialized;

        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        public void Initialize()
        {
            lock (_initializationLock)
            {
                if (_initialized)
                    return;
                DoInitialize();

                _initialized = true;
            }
        }

        void DoInitialize()
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
            var client = _client.CreateBlobClient();
            folders
                .AsParallel()
                .WithDegreeOfParallelism(folders.Count)
                .ForAll(t => client
                    .GetContainerReference(t)
                    .CreateIfNotExist());
        }

        readonly IAzureAtomicStorageStrategy _strategy;
        readonly IAzureClientConfiguration _client;


        public AzureAtomicStorageFactory(IAzureAtomicStorageStrategy strategy, IAzureClientConfiguration client)
        {
            _strategy = strategy;
            _client = client;
        }

        public NuclearStorage CreateSimplifiedStorage(bool dontInitialize = false)
        {
            var storage = new NuclearStorage(this);
            if (!dontInitialize)
            {
                Initialize();
            }
            return storage;
        }
    }
}