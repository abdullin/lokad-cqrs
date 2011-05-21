#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static void Fast(Predicate<string> name, params IAzureStorageConfiguration[] configurations)
        {
            var items = configurations
                .AsParallel()
                .Select(c => c.CreateBlobClient())
                .SelectMany(c => c.ListContainers().Where(_ => name(_.Name)))
                .Select(c => Task.Factory.FromAsync(c.BeginDelete, EndDelete(c), null));

            var queues = configurations
                .AsParallel()
                .Select(c => c.CreateQueueClient())
                .SelectMany(c => c.ListQueues().Where(_ => name(_.Name)))
                .Select(c => Task.Factory.FromAsync(c.BeginDelete, EndDelete(c), null));

            var all = items.Concat(queues).ToArray();

            Task.WaitAll(all);
        }
    }
}