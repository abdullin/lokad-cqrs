#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageInitialization : IEngineProcess
    {
        readonly IEnumerable<IAtomicStorageFactory> _storage;
        readonly ISystemObserver _observer;

        public AtomicStorageInitialization(IEnumerable<IAtomicStorageFactory> storage, ISystemObserver observer)
        {
            _storage = storage;
            _observer = observer;
        }

        public void Dispose() {}

        public void Initialize()
        {
            foreach (var atomicStorageFactory in _storage)
            {
                var folders = atomicStorageFactory.Initialize();
                if (folders.Any())
                {
                    _observer.Notify(new AtomicStorageInitialized(folders.ToArray(), atomicStorageFactory.GetType()));
                }
            }
        }

        public Task Start(CancellationToken token)
        {
            // don't do anything
            return new Task(() => { });
        }
    }
}