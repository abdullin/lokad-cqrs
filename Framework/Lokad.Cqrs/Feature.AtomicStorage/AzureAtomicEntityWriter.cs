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

        public Maybe<TEntity> Get(string key)
        {
            var blob = GetBlobReference(key);
            string text;
            try
            {
                text = blob.DownloadText();
            }
            catch (StorageClientException ex)
            {
                return Maybe<TEntity>.Empty;
            }
            return _convention.Deserialize<TEntity>(text);
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

        public TEntity AddOrUpdate(string key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            return AddOrUpdate(key, addFactory, entity =>
                {
                    update(entity);
                    return entity;
                });
        }

        public TEntity AddOrUpdate(string key, TEntity newView, Action<TEntity> updateViewFactory)
        {
            return AddOrUpdate(key, () => newView, view =>
                {
                    updateViewFactory(view);
                    return view;
                });
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

        public TEntity UpdateOrAdd(string key, Action<TEntity> update, Func<TEntity> ifNone)
        {
            return UpdateOrAdd(key, entity =>
                {
                    update(entity);
                    return entity;
                }, ifNone);
        }

        public TEntity UpdateOrThrow(string key, Action<TEntity> change)
        {
            var blob = GetBlobReference(key);
            string text;
            try
            {
                text = blob.DownloadText();
            }
            catch (StorageClientException ex)
            {
                var error = string.Format("Failed to load view {0}-{1}", typeof (TEntity), key);
                throw new InvalidOperationException(error, ex);
            }
            var view = _convention.Deserialize<TEntity>(text);
            change(view);
            blob.UploadText(_convention.Serialize(view));
            return view;
        }

        public TEntity UpdateOrThrow(string key, Func<TEntity, TEntity> change)
        {
            throw new NotImplementedException();
        }

        public bool TryUpdate(string key, Action<TEntity> change)
        {
            var blob = GetBlobReference(key);
            string downloadText;
            try
            {
                downloadText = blob.DownloadText();
            }
            catch (StorageClientException e)
            {
                return false;
            }
            var view = _convention.Deserialize<TEntity>(downloadText);
            change(view);
            blob.UploadText(_convention.Serialize(view));
            return true;
        }

        public void Delete(string key)
        {
            var blob = GetBlobReference(key);
            blob.DeleteIfExists();
        }

        CloudBlob GetBlobReference(string key)
        {
            var name =  _convention.GetNameForEntity(typeof(TEntity), key);
            return _container.GetBlobReference(name);
        }
    }
}