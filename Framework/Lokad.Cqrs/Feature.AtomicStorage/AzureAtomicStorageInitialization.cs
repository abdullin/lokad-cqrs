#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    sealed class AzureAtomicStorageInitialization : IEngineProcess
    {
        readonly CloudBlobClient _client;
        readonly IAzureAtomicStorageStrategy _strategy;

        public AzureAtomicStorageInitialization(CloudBlobClient client, IAzureAtomicStorageStrategy strategy)
        {
            _client = client;
            _strategy = strategy;
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
            // initialize views

            var types = _strategy.GetSerializableTypes();

            var folders = types
                .Select(t => _strategy.GetFolderForEntity(t))
                .ToSet();

            folders.Add(_strategy.GetFolderForSingleton());
            folders
                .AsParallel()
                .ForAll(t => _client.GetContainerReference(t).CreateIfNotExist());
        }

        public Task Start(CancellationToken token)
        {
            // don't do anything
            return new Task(() => { });
        }
    }
}