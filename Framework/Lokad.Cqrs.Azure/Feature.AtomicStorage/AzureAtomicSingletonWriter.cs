#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicSingletonWriter<TView> : IAtomicSingletonWriter<TView>
    {
        readonly IAzureStorageConfiguration _storage;
        readonly IAzureAtomicStorageStrategy _strategy;

        public AzureAtomicSingletonWriter(IAzureStorageConfiguration storage, IAzureAtomicStorageStrategy strategy)
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
                var data = blob.DownloadByteArray();
                view = _strategy.Deserialize<TView>(data);
                view = updateFactory(view);
            }
            catch (StorageClientException ex)
            {
                view = addFactory();
            }

            blob.UploadByteArray(_strategy.Serialize(view));
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