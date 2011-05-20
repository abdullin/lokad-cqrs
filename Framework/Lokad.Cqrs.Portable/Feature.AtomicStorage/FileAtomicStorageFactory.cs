using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class FileAtomicStorageFactory : IAtomicStorageFactory
    {
        readonly string _folderPath;
        readonly IAtomicStorageStrategy _strategy;

        public FileAtomicStorageFactory(string folderPath, IAtomicStorageStrategy strategy)
        {
            _folderPath = folderPath;
            _strategy = strategy;
        }

        public IAtomicEntityWriter<TKey, TEntity> GetEntityWriter<TKey, TEntity>()
        {
            return new FileAtomicEntityContainer<TKey, TEntity>(_folderPath, _strategy);
        }

        public IAtomicEntityReader<TKey, TEntity> GetEntityReader<TKey, TEntity>()
        {
            return new FileAtomicEntityContainer<TKey, TEntity>(_folderPath, _strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
            return new FileAtomicSingletonContainer<TSingleton>(_folderPath, _strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
            return new FileAtomicSingletonContainer<TSingleton>(_folderPath, _strategy);
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
                    string.Format("FileAtomicStorage was configured, but without any entity or singleton definitions. Check info on your strategy: {0}", _strategy.GetType()));
            }

            foreach (var type in entityTypes)
            {
                var folder = _strategy.GetFolderForEntity(type);
                folders.Add(folder);
            }

            folders.Add(_strategy.GetFolderForSingleton());
            foreach (var folder in folders)
            {
                Directory.CreateDirectory(Path.Combine(_folderPath, folder));
            }
        }
    }
}