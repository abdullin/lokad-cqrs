using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : BuildSyntaxHelper, IModule
    {
        readonly List<IModule> _modules = new List<IModule>();

        public void AtomicIsInMemory()
        {
            _modules.Add(new MemoryAtomicStorageModule());
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
                _modules.Add(new MemoryAtomicStorageModule());
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