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
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    public sealed class AzureAtomicEntityReader<TKey, TEntity> :
        IAtomicEntityReader<TKey, TEntity>
        //where TEntity : IAtomicEntity<TKey>
    {
        readonly CloudBlobContainer _container;
        readonly IAzureAtomicStorageStrategy _strategy;

        string ComposeName(TKey key)
        {
            return _strategy.GetNameForEntity(typeof (TEntity), key);
        }

        public AzureAtomicEntityReader(IAzureClientConfiguration client, IAzureAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            var containerName = strategy.GetFolderForEntity(typeof (TEntity));
            _container = client.CreateBlobClient().GetContainerReference(containerName);
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var blob = _container.GetBlobReference(ComposeName(key));
            byte[] data;
            try
            {
                data = blob.DownloadByteArray(new BlobRequestOptions
                    {
                        RetryPolicy = RetryPolicies.NoRetry(),
                        Timeout = TimeSpan.FromSeconds(3)
                    });
            }
            catch (StorageClientException ex)
            {
                entity = default(TEntity);
                return false;
            }

            entity = _strategy.Deserialize<TEntity>(data);
            return true;
        }
    }
}