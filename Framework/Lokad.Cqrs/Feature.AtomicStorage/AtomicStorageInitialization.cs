#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageInitialization : IEngineProcess
    {
        readonly IEnumerable<IAtomicStorageFactory> _storage;
        public AtomicStorageInitialization(IEnumerable<IAtomicStorageFactory> storage)
        {
            _storage = storage;
        }

        public void Dispose() {}

        public void Initialize()
        {
            foreach (var atomicStorageFactory in _storage)
            {
                atomicStorageFactory.Initialize();
            }
        }

        public Task Start(CancellationToken token)
        {
            // don't do anything
            return new Task(() => { });
        }
    }
}