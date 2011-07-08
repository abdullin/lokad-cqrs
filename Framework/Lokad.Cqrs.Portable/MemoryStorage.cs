#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.TapeStorage;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs
{
    using MemStore = ConcurrentDictionary<string, byte[]>;

    public static class MemoryStorage
    {
        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage, using the default
        /// storage configuration and atomic strategy.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// new instance of the nuclear storage
        /// </returns>
        public static NuclearStorage CreateNuclear(MemStore dictionary)
        {
            return CreateNuclear(dictionary, b => { });
        }

        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage, using the default
        /// storage configuration and atomic strategy.
        /// </summary>
        /// <returns>
        /// new instance of the nuclear storage
        /// </returns>
         public static NuclearStorage CreateNuclear()
        {
            return CreateNuclear(new MemStore(), b => { });
        }

        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="configStrategy">The config strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(MemStore dictionary,
            Action<DefaultAtomicStorageStrategyBuilder> configStrategy)
        {
            var strategyBuilder = new DefaultAtomicStorageStrategyBuilder();
            configStrategy(strategyBuilder);
            var strategy = strategyBuilder.Build();
            return CreateNuclear(dictionary, strategy);
        }


        /// <summary>
        /// Creates the simplified nuclear storage wrapper around Atomic storage.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="strategy">The atomic storage strategy.</param>
        /// <returns></returns>
        public static NuclearStorage CreateNuclear(MemStore dictionary, IAtomicStorageStrategy strategy)
        {
            var factory = new MemoryAtomicStorageFactory(dictionary, strategy);
            factory.Initialize();
            return new NuclearStorage(factory);
        }

        /// <summary>
        /// Creates memory-based tape storage factory.
        /// </summary>
        /// <returns></returns>
        public static MemoryTapeStorageFactory CreateTape()
        {
            var dictionary = new ConcurrentDictionary<string, List<byte[]>>();

            return CreateTape(dictionary);
        }

        /// <summary>
        /// Creates memory-based tape storage factory, using the provided concurrent dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static MemoryTapeStorageFactory CreateTape(ConcurrentDictionary<string, List<byte[]>> dictionary)
        {
            var factory = new MemoryTapeStorageFactory(dictionary);
            factory.InitializeForWriting();
            return factory;
        }
    }
}