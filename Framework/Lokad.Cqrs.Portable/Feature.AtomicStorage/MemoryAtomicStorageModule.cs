using System.Collections.Concurrent;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicStorageModule : IModule
    {
        readonly IAtomicStorageStrategy _strategy;

        public MemoryAtomicStorageModule(IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
        }


        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(_strategy);
            var store = new ConcurrentDictionary<string, byte[]>();
            builder.RegisterInstance(store);
            builder
                .RegisterInstance(new MemoryAtomicStorageFactory(store, _strategy))
                .As<IAtomicStorageFactory>();
            

            builder
                .RegisterGeneric(typeof (MemoryAtomicEntityContainer<,>))
                .As(typeof (IAtomicEntityReader<,>))
                .As(typeof (IAtomicEntityWriter<,>))
                .SingleInstance();

            builder
                .RegisterGeneric(typeof (MemoryAtomicSingletonContainer<>))
                .As(typeof (IAtomicSingletonReader<>))
                .As(typeof (IAtomicSingletonWriter<>))
                .SingleInstance();
            

            builder.Update(componentRegistry);
        }
    }

    
}