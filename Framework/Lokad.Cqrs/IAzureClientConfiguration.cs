using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs
{
    public interface IAzureClientConfiguration
    {
        string AccountName { get; }
        CloudBlobContainer BuildContainer(string containerName);
        CloudQueue BuildQueue(string queueName);
    }

    
}