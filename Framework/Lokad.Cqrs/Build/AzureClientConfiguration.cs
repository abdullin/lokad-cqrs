using System;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Build
{
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

        public CloudBlobContainer GetContainerReference(string containerName)
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