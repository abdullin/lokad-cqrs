#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicSingletonReader<TView> : IAtomicSingletonReader<TView>
    {
        readonly IAzureStorageConfiguration _storage;
        readonly IAzureAtomicStorageStrategy _strategy;

        public AzureAtomicSingletonReader(IAzureStorageConfiguration storage, IAzureAtomicStorageStrategy strategy)
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


        public bool TryGet(out TView view)
        {
            var blob = GetBlob();
            byte[] data;
            try
            {
                // no retries and small timeout
                data = blob.DownloadByteArray(new BlobRequestOptions
                    {
                        RetryPolicy = RetryPolicies.NoRetry(),
                        Timeout = TimeSpan.FromSeconds(3)
                    });
            }
            catch (StorageClientException ex)
            {
                view = default(TView);
                return false;
            }
            view = _strategy.Deserialize<TView>(data);
            return true;
        }
    }
}