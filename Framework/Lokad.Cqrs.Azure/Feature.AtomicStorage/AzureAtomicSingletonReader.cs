#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicSingletonReader<TView> : IAtomicSingletonReader<TView>
    {
        readonly IAzureStorageConfig _storage;
        readonly IAtomicStorageStrategy _strategy;

        public AzureAtomicSingletonReader(IAzureStorageConfig storage, IAtomicStorageStrategy strategy)
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
            try
            {
                // no retries and small timeout
                using(var data = blob.OpenRead(new BlobRequestOptions
                    {
                        RetryPolicy = RetryPolicies.NoRetry(),
                        Timeout = TimeSpan.FromSeconds(3)
                    }))
                {
                    view = _strategy.Deserialize<TView>(data);
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
                        view = default(TView);
                        return false;
                    default:
                        throw;
                }
            }
        }
    }
}