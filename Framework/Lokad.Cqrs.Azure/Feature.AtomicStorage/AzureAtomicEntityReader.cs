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
        readonly IAtomicStorageStrategy _strategy;

        string ComposeName(TKey key)
        {
            return _strategy.GetNameForEntity(typeof (TEntity), key);
        }

        public AzureAtomicEntityReader(IAzureStorageConfig storage, IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            var containerName = strategy.GetFolderForEntity(typeof (TEntity));
            _container = storage.CreateBlobClient().GetContainerReference(containerName);
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var blob = _container.GetBlobReference(ComposeName(key));
            try
            {
                // blob request options are cloned from the config
                using (var data = blob.OpenRead())
                {
                    entity = _strategy.Deserialize<TEntity>(data);
                    return true;
                }
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        entity = default(TEntity);
                        return false;
                    default:
                        throw;
                }
            }
        }
    }
}