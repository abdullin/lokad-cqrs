using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;
using Lokad.Cqrs.Feature.TapeStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : HideObjectMembersFromIntelliSense
    {
        IAtomicStorageFactory _atomicStorageFactory;
        IStreamingRoot _streamingRoot;

        ITapeStorageFactory _tapeStorage;

        
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
        public void TapeIs(ITapeStorageFactory storage)
        {
            _tapeStorage = storage;
        }

        public void TapeIsInMemory()
        {
            var storage = new ConcurrentDictionary<string, List<byte[]>>();
            var factory = new MemoryTapeStorageFactory(storage);
            TapeIs(factory);
        }

        public void TapeIsInFiles(string fullPath)
        {
            var factory = new FileTapeStorageFactory(fullPath);
            TapeIs(factory);
        }

        public void AtomicIsInFiles(string folder)
        {
            AtomicIsInFiles(folder, builder => { });
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


        public void Configure(IComponentRegistry componentRegistry)
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
            if (_tapeStorage == null)
            {
                TapeIsInMemory();
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

            builder.RegisterInstance(_tapeStorage);
            builder.RegisterInstance(new TapeStorageInitilization(new[] {_tapeStorage})).As<IEngineProcess>();

            builder.Update(componentRegistry);
        }
    }
}