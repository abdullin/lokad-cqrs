#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Azure implementation of the view reader/writer
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    public sealed class AzureAtomicEntityReader<TKey, TView> :
        IAtomicEntityReader<TKey, TView>
        //where TView : IAtomicEntity<TKey>
    {
        readonly CloudBlobContainer _container;
        readonly IAzureAtomicStorageStrategy _strategy;

        string ComposeName(TKey key)
        {
            return _strategy.GetNameForEntity(key);
        }

        public AzureAtomicEntityReader(CloudBlobClient client, IAzureAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            var containerName = strategy.GetFolderForEntity(typeof(TView));
            _container = client.GetContainerReference(containerName);
        }

        public Maybe<TView> Get(TKey key)
        {
            var blob = _container.GetBlobReference(ComposeName(key));
            string text;
            try
            {
                text = blob.DownloadText(new BlobRequestOptions
                    {
                        RetryPolicy = RetryPolicies.NoRetry(),
                        Timeout = TimeSpan.FromSeconds(3)
                    });
            }
            catch (StorageClientException ex)
            {
                return Maybe<TView>.Empty;
            }
            return _strategy.Deserialize<TView>(text);
        }

        public TView Load(TKey key)
        {
            return Get(key).ExposeException("Failed to load '{0}' with key '{1}'.", typeof (TView).Name, key);
        }
    }
}