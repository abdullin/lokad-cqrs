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
        readonly CloudBlobClient _client;
        readonly IAzureAtomicStorageStrategy _strategy;

        public AzureAtomicSingletonReader(CloudBlobClient client, IAzureAtomicStorageStrategy strategy)
        {
            _client = client;
            _strategy = strategy;
        }

        CloudBlob GetBlob()
        {
            var name = _strategy.GetNameForSingleton(typeof (TView));

            return _client
                .GetContainerReference(_strategy.GetFolderForSingleton())
                .GetBlobReference(name);
        }


        public Maybe<TView> Get()
        {
            var blob = GetBlob();
            string text;
            try
            {
                // no retries and small timeout
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
    }
}