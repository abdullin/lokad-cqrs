using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : HideObjectMembersFromIntelliSense, IModule
    {
        IAtomicStorageFactory _atomicStorageFactory;

        public void AtomicIs(IAtomicStorageFactory factory)
        {
            _atomicStorageFactory = factory;
        }

        //public void AtomicIs(Func<IComponentContext>)
        public void AtomicIsInMemory()
        {
            AtomicIsInMemory(builder => { });
        }

        public void AtomicIsInMemory(Action<DefaultAtomicStorageStrategyBuilder> configure)
        {
            var dictionary = new ConcurrentDictionary<string, byte[]>();
            var builder = new DefaultAtomicStorageStrategyBuilder();
            configure(builder);
            AtomicIs(new MemoryAtomicStorageFactory(dictionary, builder.Build()));
        }

        public void AtomicIsInFiles(string folder, Action<DefaultAtomicStorageStrategyBuilder> configure)
        {
            var builder = new DefaultAtomicStorageStrategyBuilder();
            configure(builder);
            AtomicIs(new FileAtomicStorageFactory(folder, builder.Build()));
        }


        readonly List<IModule> _modules = new List<IModule>();


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
            var builder = new ContainerBuilder();

            if (_atomicStorageFactory == null)
            {
                AtomicIsInMemory(strategyBuilder => { });
            }
            

            var source = new AtomicRegistrationSource(_atomicStorageFactory);
            builder.RegisterSource(source);
            builder.RegisterInstance(new NuclearStorage(_atomicStorageFactory));
            builder.RegisterType<AtomicStorageInitialization>().As<IEngineProcess>().SingleInstance();
            
            builder.Update(componentRegistry);
        }
    }

}