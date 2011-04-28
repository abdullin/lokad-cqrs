#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Windows Azure implementation of storage 
    /// </summary>
    public sealed class BlobStorageRoot : IStorageRoot
    {
        readonly CloudBlobClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageRoot"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public BlobStorageRoot(CloudBlobClient client)
        {
            _client = client;
        }

        public IStorageContainer GetContainer(string name)
        {
            return new BlobStorageContainer(_client.GetBlobDirectoryReference(name));
        }
    }
}