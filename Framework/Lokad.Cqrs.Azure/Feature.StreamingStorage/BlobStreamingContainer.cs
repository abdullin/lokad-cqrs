#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.StorageClient;
using System.Linq;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Windows Azure implementation of storage 
    /// </summary>
    public sealed class BlobStreamingContainer : IStreamingContainer
    {
        readonly CloudBlobDirectory _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStreamingContainer"/> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public BlobStreamingContainer(CloudBlobDirectory directory)
        {
            _directory = directory;
        }

        public IStreamingContainer GetContainer(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return new BlobStreamingContainer(_directory.GetSubdirectory(name));
        }

        public IStreamingItem GetItem(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            return new BlobStreamingItem(_directory.GetBlobReference(name));
        }

        public IStreamingContainer Create()
        {
            _directory.Container.CreateIfNotExist();
            return this;
        }

        public void Delete()
        {
            try
            {
                _directory.Container.Delete();
            }
            catch (StorageClientException e)
            {
                switch (e.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        return;
                    default:
                        throw;
                }
            }
        }

        public IEnumerable<string> ListItems()
        {
            try
            {
                return _directory.ListBlobs()
                    .Select(item => _directory.Uri.MakeRelativeUri(item.Uri).ToString())
                    .ToArray();
            }
            catch (StorageClientException e)
            {
                switch (e.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        throw StreamingErrors.ContainerNotFound(this, e);
                    default:
                        throw;
                }
            }
        }

        public bool Exists()
        {
            try
            {
                _directory.Container.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                switch (e.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                    case StorageErrorCode.ResourceNotFound:
                    case StorageErrorCode.BlobNotFound:
                        return false;
                    default:
                        throw;
                }
            }
        }

        public string FullPath
        {
            get { return _directory.Uri.ToString(); }
        }
    }
}