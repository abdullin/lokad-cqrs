#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Build
{
    public sealed class AzureClientConfigurationBuilder
    {
        readonly CloudStorageAccount _account;

        Action<CloudQueueClient> _queueClientConfiguration;
        Action<CloudBlobClient> _blobClientConfiguration;
        readonly string _accountId;


        public AzureClientConfigurationBuilder ConfigureQueueClient(Action<CloudQueueClient> configure,
            bool replaceOld = false)
        {
            if (replaceOld)
            {
                _queueClientConfiguration = configure;
            }
            else
            {
                _queueClientConfiguration += configure;
            }
            return this;
        }

        public AzureClientConfigurationBuilder ConfigureBlobClient(Action<CloudBlobClient> configure,
            bool replaceOld = false)
        {
            if (replaceOld)
            {
                _blobClientConfiguration = configure;
            }
            else
            {
                _blobClientConfiguration += configure;
            }
            return this;
        }


        public AzureClientConfigurationBuilder(CloudStorageAccount account, string accountId)
        {
            // defaults
            _queueClientConfiguration = client => client.RetryPolicy = RetryPolicies.NoRetry();
            _blobClientConfiguration = client => client.RetryPolicy = RetryPolicies.NoRetry();
            _account = account;
            _accountId = accountId;
        }

        internal AzureClientConfiguration Build()
        {
            return new AzureClientConfiguration(_account, _queueClientConfiguration, _blobClientConfiguration,
                _accountId);
        }
    }

    sealed class AzureClientConfiguration : IAzureClientConfiguration
    {
        readonly Action<CloudQueueClient> _queueClientConfiguration;
        readonly Action<CloudBlobClient> _blobClientConfiguration;
        public readonly CloudStorageAccount Account;
        readonly string _accountName;

        static void DisableNagleForQueuesAndTables(CloudStorageAccount account)
        {
            // http://blogs.msdn.com/b/windowsazurestorage/archive/2010/06/25/nagle-s-algorithm-is-not-friendly-towards-small-requests.aspx
            // improving transfer speeds for the small requests
            ServicePointManager.FindServicePoint(account.TableEndpoint).UseNagleAlgorithm = false;
            ServicePointManager.FindServicePoint(account.QueueEndpoint).UseNagleAlgorithm = false;
        }

        public AzureClientConfiguration(CloudStorageAccount account, Action<CloudQueueClient> queueClientConfiguration,
            Action<CloudBlobClient> blobClientConfiguration, string customName)
        {
            _queueClientConfiguration = queueClientConfiguration;
            _blobClientConfiguration = blobClientConfiguration;
            Account = account;
            _accountName = customName ?? account.Credentials.AccountName;
            DisableNagleForQueuesAndTables(account);
        }

        public string AccountName
        {
            get { return _accountName; }
        }

        public CloudBlobContainer BuildContainer(string containerName)
        {
            var client = Account.CreateCloudBlobClient();
            _blobClientConfiguration(client);
            return client.GetContainerReference(containerName);
        }

        public CloudQueue BuildQueue(string queueName)
        {
            var client = Account.CreateCloudQueueClient();
            _queueClientConfiguration(client);
            return client.GetQueueReference(queueName);
        }
    }
}