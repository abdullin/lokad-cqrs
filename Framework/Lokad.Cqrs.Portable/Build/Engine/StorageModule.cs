using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : HideObjectMembersFromIntelliSense, IModule
    {
        readonly List<IModule> _modules = new List<IModule>();

        public void AtomicIsInMemory(Action<DefaultAtomicStorageStrategyBuilder> config)
        {
            var builder = new DefaultAtomicStorageStrategyBuilder();
            config(builder);
            
            _modules.Add(new MemoryAtomicStorageModule(builder.Build()));
        }

        public void AtomicIsInMemory(IAtomicStorageStrategy strategy)
        {
            _modules.Add(new MemoryAtomicStorageModule(strategy));
        }

        public void AtomicIsInMemory()
        {
            AtomicIsInMemory(builder => { });
        }

        public void StreamingIsInFiles(string filePath)
        {
            _modules.Add(new FileStreamingStorageModule(filePath));
        }

        public void StreamingIsInFiles(string filePath, Action<FileStreamingStorageModule> config)
        {
            var module = new FileStreamingStorageModule(filePath);
            config(module);
            _modules.Add(module);
        }

        public void EnlistModule(IModule module)
        {
            _modules.Add(module);
        }


        void IModule.Configure(IComponentRegistry componentRegistry)
        {
            if (_modules.Count == 0)
            {
                AtomicIsInMemory();
            }

            var builder = new ContainerBuilder();
            foreach (var module in _modules)
            {
                builder.RegisterModule(module);
            }
            builder.Update(componentRegistry);
        }
    }
}