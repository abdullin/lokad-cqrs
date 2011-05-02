using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicStorageModule : IModule
    {
        readonly MemoryAtomicStorageStrategy _strategy = new MemoryAtomicStorageStrategy();

        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(_strategy);
            builder
                .RegisterInstance(new MemoryAtomicStorageFactory(_strategy))
                .As<IAtomicStorageFactory>();


            builder
                .RegisterGeneric(typeof (MemoryAtomicEntityContainer<>))
                .As(typeof (IAtomicEntityReader<>))
                .As(typeof (IAtomicEntityWriter<>))
                .SingleInstance();

            builder
                .RegisterGeneric(typeof (MemoryAtomicSingletonContainer<>))
                .As(typeof (IAtomicSingletonReader<>))
                .As(typeof (IAtomicSingletonWriter<>))
                .SingleInstance();
            builder
                .RegisterType<NuclearStorage>().SingleInstance();

            builder.Update(componentRegistry);
        }
    }
}