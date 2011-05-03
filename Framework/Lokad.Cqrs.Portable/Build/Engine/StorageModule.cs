using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : BuildSyntaxHelper, IModule
    {
        readonly List<IModule> _modules = new List<IModule>();

        public void AtomicIsInMemory()
        {
            _modules.Add(new MemoryAtomicStorageModule());
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