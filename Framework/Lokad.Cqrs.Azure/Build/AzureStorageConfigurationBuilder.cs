#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Build
{
    public sealed class AzureStorageConfigurationBuilder
    {
        readonly CloudStorageAccount _account;

        Action<CloudQueueClient> _queueConfig;
        Action<CloudBlobClient> _blobConfig;
        Action<CloudTableClient> _tableConfig;
        readonly string _accountId;
        bool _cleanAccount;


        public AzureStorageConfigurationBuilder ConfigureQueueClient(Action<CloudQueueClient> configure,
            bool replaceOld = false)
        {
            if (replaceOld)
            {
                _queueConfig = configure;
            }
            else
            {
                _queueConfig += configure;
            }
            return this;
        }

        public AzureStorageConfigurationBuilder ConfigureBlobClient(Action<CloudBlobClient> configure,
            bool replaceOld = false)
        {
            if (replaceOld)
            {
                _blobConfig = configure;
            }
            else
            {
                _blobConfig += configure;
            }
            return this;
        }

        public AzureStorageConfigurationBuilder ConfigureTableClient(Action<CloudTableClient> configure,
    bool replaceOld = false)
        {
            if (replaceOld)
            {
                _tableConfig = configure;
            }
            else
            {
                _tableConfig += configure;
            }
            return this;
        }


        public AzureStorageConfigurationBuilder(CloudStorageAccount account, string accountId)
        {
            // defaults
            _queueConfig = client => client.RetryPolicy = RetryPolicies.NoRetry();
            _blobConfig = client =>
                {
                    client.RetryPolicy = RetryPolicies.NoRetry();
                };
            _tableConfig = client => client.RetryPolicy = RetryPolicies.NoRetry();

            
            if (account.Credentials.AccountName == "devstoreaccount1")
            {
                _blobConfig += client =>
                    {
                        // http://stackoverflow.com/questions/4897826/
                        // local dev store works poorly with multi-thread uploads
                        client.ParallelOperationThreadCount = 1;
                    };
            }

            _account = account;
            _accountId = accountId;
        }

        public void CleanAtStartup()
        {
            _cleanAccount = true;
        }

        internal AzureStorageConfiguration Build()
        {
            return new AzureStorageConfiguration(_account, 
                _queueConfig, 
                _blobConfig, 
                _tableConfig,
                _accountId);
        }
    }
}