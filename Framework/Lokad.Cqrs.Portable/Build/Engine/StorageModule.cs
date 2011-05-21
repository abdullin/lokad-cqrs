using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : HideObjectMembersFromIntelliSense, IModule
    {
        IAtomicStorageFactory _atomicStorageFactory;
        IStreamingRoot _streamingRoot;

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
            _streamingRoot = new FileStreamingContainer(filePath);
        }

        public void StreamingIs(IStreamingRoot streamingRoot)
        {
            _streamingRoot = streamingRoot;
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
            if (_streamingRoot == null)
            {
                StreamingIsInFiles(Directory.GetCurrentDirectory());
            }

            var core = new AtomicRegistrationCore(_atomicStorageFactory);
            var source = new AtomicRegistrationSource(core);
            builder.RegisterSource(source);
            builder.RegisterInstance(new NuclearStorage(_atomicStorageFactory));
            builder
                .Register(
                    c => new AtomicStorageInitialization(new[] {_atomicStorageFactory}, c.Resolve<ISystemObserver>()))
                .As<IEngineProcess>().SingleInstance();

            builder.RegisterInstance(_streamingRoot);
            builder.Update(componentRegistry);
        }
    }

}