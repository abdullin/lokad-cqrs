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
    public sealed class AzureAtomicEntityWriter<TEntity> :
        IAtomicEntityWriter<TEntity>
        //where TEntity : IAtomicEntity<TKey>
    {
        readonly CloudBlobContainer _container;
        readonly IAzureAtomicStorageStrategy _convention;

        public AzureAtomicEntityWriter(CloudBlobClient client, IAzureAtomicStorageStrategy convention)
        {
            var containerName = _convention.GetFolderForEntity(typeof (TEntity));
            _container = client.GetContainerReference(containerName);
            _convention = convention;
        }

        public TEntity AddOrUpdate(string key, Func<TEntity> addViewFactory, Func<TEntity,TEntity> updateViewFactory)
        {
            // TODO: implement proper locking and order
            var blob = GetBlobReference(key);
            TEntity view;
            try
            {
                var downloadText = blob.DownloadText();
                view = _convention.Deserialize<TEntity>(downloadText);
                view = updateViewFactory(view);
            }
            catch (StorageClientException ex)
            {
                view = addViewFactory();
            }

            blob.UploadText(_convention.Serialize(view));
            return view;
        }

        

        public TEntity UpdateOrAdd(string key, Func<TEntity, TEntity> update, Func<TEntity> ifNone)
        {
            // TODO: implement proper locking and order
            var blob = GetBlobReference(key);
            TEntity entity;
            try
            {
                var text = blob.DownloadText();
                var source = _convention.Deserialize<TEntity>(text);
                entity = update(source);
            }
            catch (StorageClientException ex)
            {
                entity = ifNone();
            }
            

            blob.UploadText(_convention.Serialize(entity));
            return entity;
        }


        


        public bool TryDelete(string key)
        {
            var blob = GetBlobReference(key);
            return blob.DeleteIfExists();
        }

        CloudBlob GetBlobReference(string key)
        {
            var name =  _convention.GetNameForEntity(typeof(TEntity), key);
            return _container.GetBlobReference(name);
        }
    }
}