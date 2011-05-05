using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lokad.Cqrs.Build;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs
{
    public static class WipeAzureAccount
    {
        static Action<IAsyncResult> EndDelete(CloudBlobContainer c)
        {
            return result =>
                {
                    try
                    {
                        c.EndDelete(result);
                    }
                    catch (StorageClientException ex)
                    {
                        if (ex.ErrorCode == StorageErrorCode.ContainerNotFound)
                            return;
                        throw;
                    }
                };
        }
        static Action<IAsyncResult> EndDelete(CloudQueue c)
        {
            return result =>
                {
                    try
                    {
                        c.EndDelete(result);
                    }
                    catch (StorageClientException ex)
                    {
                        if (ex.ErrorCode == StorageErrorCode.ResourceNotFound)
                            return;
                        throw;
                    }
                };
        }

        public static void Fast(IEnumerable<IAzureStorageConfiguration> configurations)
        {
            var items = configurations
                .AsParallel()
                .Select(c => c.CreateBlobClient())
                .SelectMany(c => c.ListContainers())
                .Select(c => Task.Factory.FromAsync(c.BeginDelete, EndDelete((CloudBlobContainer) c), null));

            var queues = configurations
                .AsParallel()
                .Select(c => c.CreateQueueClient())
                .SelectMany(c => c.ListQueues())
                .Select(c => Task.Factory.FromAsync(c.BeginDelete, EndDelete(c), null));

            var all = items.Concat(queues).ToArray();

            Task.WaitAll(all);
        }
    }
}