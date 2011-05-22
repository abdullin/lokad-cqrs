#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicSingletonWriter<TView> : IAtomicSingletonWriter<TView>
    {
        readonly IAzureStorageConfig _storage;
        readonly IAtomicStorageStrategy _strategy;

        public AzureAtomicSingletonWriter(IAzureStorageConfig storage, IAtomicStorageStrategy strategy)
        {
            _storage = storage;
            _strategy = strategy;
        }

        CloudBlob GetBlob()
        {
            var name = _strategy.GetNameForSingleton(typeof (TView));

            return _storage
                .CreateBlobClient()
                .GetContainerReference(_strategy.GetFolderForSingleton())
                .GetBlobReference(name);
        }


        public TView AddOrUpdate(Func<TView> addFactory, Func<TView,TView> updateFactory, AddOrUpdateHint hint)
        {
            // TODO: switch to etags and use hint
            var blob = GetBlob();

            TView view;
            try
            {
                using (var mem = blob.OpenRead())
                {
                    view = _strategy.Deserialize<TView>(mem);
                }
                view = updateFactory(view);
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        var s = string.Format(
                            "Container '{0}' does not exist. You need to initialize this atomic storage and ensure that '{1}' is known to '{2}'.",
                            blob.Container.Name, typeof(TView).Name, _strategy.GetType().Name);
                        throw new InvalidOperationException(s, ex);
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ResourceNotFound:
                        view = addFactory();
                        break;
                    default:
                        throw;
                }
            }

            using (var write = blob.OpenWrite())
            {
                _strategy.Serialize(view, write);
            }
            return view;
        }

        public TView AddOrUpdate(Func<TView> addFactory, Action<TView> update, AddOrUpdateHint hint)
        {
            return AddOrUpdate(addFactory, view =>
                {
                    update(view);
                    return view;
                }, hint);
        }

      

        public TView UpdateOrThrow(Action<TView> update)
        {
            return AddOrUpdate(() => { throw new InvalidOperationException("View not found"); }, update, AddOrUpdateHint.ProbablyExists);
        }

        public TView UpdateOrThrow(Func<TView, TView> update)
        {
            return AddOrUpdate(() => { throw new InvalidOperationException("View not found"); }, update, AddOrUpdateHint.ProbablyExists);
        }

        public bool TryDelete()
        {
            return GetBlob().DeleteIfExists();
        }

    }
}