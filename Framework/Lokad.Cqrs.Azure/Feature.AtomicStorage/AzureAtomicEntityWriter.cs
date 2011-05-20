#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Azure implementation of the view reader/writer
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    public sealed class AzureAtomicEntityWriter<TKey, TEntity> :
        IAtomicEntityWriter<TKey, TEntity>
        //where TEntity : IAtomicEntity<TKey>
    {
        readonly CloudBlobContainer _container;
        readonly IAtomicStorageStrategy _strategy;

        public AzureAtomicEntityWriter(IAzureAccessConfiguration access, IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            var containerName = strategy.GetFolderForEntity(typeof (TEntity));
            _container = access.CreateBlobClient().GetContainerReference(containerName);
            
        }

        public TEntity AddOrUpdate(TKey key, Func<TEntity> addViewFactory, Func<TEntity,TEntity> updateViewFactory, AddOrUpdateHint hint)
        {
            // TODO: implement proper locking and order
            var blob = GetBlobReference(key);
            TEntity view;
            try
            {
                using (var stream = blob.OpenRead())
                {
                    view = _strategy.Deserialize<TEntity>(stream);
                }
                
                view = updateViewFactory(view);
            }
            catch (StorageClientException ex)
            {
                switch(ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        var s = string.Format(
                            "Container '{0}' does not exist. You need to initialize this atomic storage and ensure that '{1}' is known to '{2}'.",
                            blob.Container.Name, typeof (TEntity).Name, _strategy.GetType().Name);
                        throw new InvalidOperationException(s, ex);
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        view = addViewFactory();
                        break;
                    default:
                        throw;

                }
            }
            using (var mem = blob.OpenWrite())
            {
                _strategy.Serialize(view, mem);
            }
            return view;
        }


        public bool TryDelete(TKey key)
        {
            var blob = GetBlobReference(key);
            return blob.DeleteIfExists();
        }

        CloudBlob GetBlobReference(TKey key)
        {
            var name =  _strategy.GetNameForEntity(typeof(TEntity), key);
            return _container.GetBlobReference(name);
        }
    }
}