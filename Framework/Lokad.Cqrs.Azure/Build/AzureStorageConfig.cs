using System;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Build
{
    sealed class AzureStorageConfig : IAzureStorageConfig
    {
        readonly Action<CloudQueueClient> _queueClientConfiguration;
        readonly Action<CloudBlobClient> _blobClientConfiguration;
        readonly Action<CloudTableClient> _tableClientConfiguration;
        public readonly CloudStorageAccount Account;
        readonly string _accountName;

        static void DisableNagleForQueuesAndTables(CloudStorageAccount account)
        {
            // http://blogs.msdn.com/b/windowsazurestorage/archive/2010/06/25/nagle-s-algorithm-is-not-friendly-towards-small-requests.aspx
            // improving transfer speeds for the small requests
            ServicePointManager.FindServicePoint(account.TableEndpoint).UseNagleAlgorithm = false;
            ServicePointManager.FindServicePoint(account.QueueEndpoint).UseNagleAlgorithm = false;
        }

        public AzureStorageConfig(CloudStorageAccount account, Action<CloudQueueClient> queueClientConfiguration,
            Action<CloudBlobClient> blobClientConfiguration, Action<CloudTableClient> tableClientConfiguration, string customName)
        {
            _queueClientConfiguration = queueClientConfiguration;
            _tableClientConfiguration = tableClientConfiguration;
            _blobClientConfiguration = blobClientConfiguration;
            Account = account;
            _accountName = customName ?? account.Credentials.AccountName;
            DisableNagleForQueuesAndTables(account);
        }

        public string AccountName
        {
            get { return _accountName; }
        }

        public CloudStorageAccount UnderlyingAccount
        {
            get { return Account; }
        }

        public CloudQueueClient CreateQueueClient()
        {
            var client = Account.CreateCloudQueueClient();
            _queueClientConfiguration(client);
            return client;
        }

        public CloudTableClient CreateTableClient()
        {
            var client = Account.CreateCloudTableClient();
            _tableClientConfiguration(client);
            return client;
        }

        public CloudBlobClient CreateBlobClient()
        {
            var client = Account.CreateCloudBlobClient();
            _blobClientConfiguration(client);
            return client;
        }
    }
}