#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Net;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Azure BLOB implementation of the <see cref="IStreamingItem"/>
    /// </summary>
    public sealed class BlobStreamingItem : IStreamingItem
    {
        readonly CloudBlob _blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStreamingItem"/> class.
        /// </summary>
        /// <param name="blob">The BLOB.</param>
        public BlobStreamingItem(CloudBlob blob)
        {
            _blob = blob;
        }

        //const string ContentCompression = "gzip";

        /// <summary>
        /// Performs the write operation, ensuring that the condition is met.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="writeOptions">The write options.</param>
        public long Write(Action<Stream> writer, StreamingCondition condition, StreamingWriteOptions writeOptions)
        {
            try
            {
                var mapped = Map(condition);

                return BlobStorageUtil.Write(mapped, _blob, writer, writeOptions);
            }
            catch (StorageServerException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ServiceIntegrityCheckFailed:
                        throw StreamingErrors.IntegrityFailure(this, ex);
                    default:
                        throw;
                }
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ConditionFailed:
                        throw StreamingErrors.ConditionFailed(this, condition, ex);
                    case StorageErrorCode.ContainerNotFound:
                        throw StreamingErrors.ContainerNotFound(this, ex);
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        /// Attempts to read the storage item.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="condition">The condition.</param>
        /// <exception cref="StreamingItemNotFoundException">if the item does not exist.</exception>
        /// <exception cref="StreamingContainerNotFoundException">if the container for the item does not exist</exception>
        /// <exception cref="StreamingItemIntegrityException">when integrity check fails</exception>
        public void ReadInto(ReaderDelegate reader, StreamingCondition condition)
        {
            try
            {
                var mapped = Map(condition);
                BlobStorageUtil.Read(mapped, _blob, reader);
            }
            catch (StreamingItemIntegrityException e)
            {
                throw StreamingErrors.IntegrityFailure(this, e);
            }
            catch (StorageClientException e)
            {
                switch (e.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        throw StreamingErrors.ContainerNotFound(this, e);
                    case StorageErrorCode.ResourceNotFound:
                    case StorageErrorCode.BlobNotFound:
                        throw StreamingErrors.ItemNotFound(this, e);
                    case StorageErrorCode.ConditionFailed:
                        throw StreamingErrors.ConditionFailed(this, condition, e);
                    case StorageErrorCode.ServiceIntegrityCheckFailed:
                        throw StreamingErrors.IntegrityFailure(this, e);
                    case StorageErrorCode.BadRequest:
                        switch (e.StatusCode)
                        {
                                // for some reason Azure Storage happens to get here as well
                            case HttpStatusCode.PreconditionFailed:
                            case HttpStatusCode.NotModified:
                                throw StreamingErrors.ConditionFailed(this, condition, e);
                            default:
                                throw;
                        }
                    default:
                        throw;
                }
            }
        }

        /// <summary>
        /// Removes the item, ensuring that the specified condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void Delete(StreamingCondition condition)
        {
            try
            {
                var options = Map(condition);
                _blob.Delete(options);
            }
            catch (StorageClientException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                        throw StreamingErrors.ContainerNotFound(this, ex);
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ConditionFailed:
                        return;
                    default:
                        throw;
                }
            }
        }

        public Optional<StreamingItemInfo> GetInfo(StreamingCondition condition)
        {
            try
            {
                _blob.FetchAttributes(Map(condition));
                return BlobStorageUtil.MapFetchedAttrbitues(_blob);
            }
            catch (StorageClientException e)
            {
                switch (e.ErrorCode)
                {
                    case StorageErrorCode.ContainerNotFound:
                    case StorageErrorCode.ResourceNotFound:
                    case StorageErrorCode.BlobNotFound:
                    case StorageErrorCode.ConditionFailed:
                        return Optional<StreamingItemInfo>.Empty;
                    case StorageErrorCode.BadRequest:
                        switch (e.StatusCode)
                        {
                            case HttpStatusCode.PreconditionFailed:
                                return Optional<StreamingItemInfo>.Empty;
                            default:
                                throw;
                        }
                }
                throw;
            }
        }

        public void CopyFrom(IStreamingItem sourceItem,
            StreamingCondition condition,
            StreamingCondition copySourceCondition,
            StreamingWriteOptions writeOptions)
        {
            var item = sourceItem as BlobStreamingItem;

            if (item != null)
            {
                try
                {
                    _blob.CopyFromBlob(item._blob, Map(condition, copySourceCondition));
                }
                catch (StorageClientException e)
                {
                    switch (e.ErrorCode)
                    {
                        case StorageErrorCode.BlobNotFound:
                            throw StreamingErrors.ItemNotFound(this, e);
                        default:
                            throw;
                    }
                }
            }
            else
            {
                // based on the default write block size of BLOB
                const int bufferSize = 0x400000;
                Write(
                    targetStream =>
                        sourceItem.ReadInto(
                        (props, stream) => stream.CopyTo(targetStream, bufferSize),
                            copySourceCondition), condition,
                    writeOptions);
            }
        }

        /// <summary>
        /// Gets the full path of the current item.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _blob.Uri.ToString(); }
        }

        /// <summary>
        /// Gets the BLOB reference behind this instance.
        /// </summary>
        /// <value>The reference.</value>
        public CloudBlob Reference
        {
            get { return _blob; }
        }

        static BlobRequestOptions Map(StreamingCondition condition,
            StreamingCondition copySourceAccessCondition = default(StreamingCondition))
        {
            if ((condition.Type == StreamingConditionType.None) &&
                (copySourceAccessCondition.Type == StreamingConditionType.None))
                return null;

            return new BlobRequestOptions
                {
                    AccessCondition = MapCondition(condition),
                    CopySourceAccessCondition = MapCondition(copySourceAccessCondition)
                };
        }

        static AccessCondition MapCondition(StreamingCondition condition)
        {
            switch (condition.Type)
            {
                case StreamingConditionType.None:
                    return AccessCondition.None;
                case StreamingConditionType.IfMatch:
                    var x = ExposeException(condition.ETag, "'ETag' should be present");
                    return AccessCondition.IfMatch(x);
                case StreamingConditionType.IfNoneMatch:
                    var etag = ExposeException(condition.ETag, "'ETag' should be present");
                    return AccessCondition.IfNoneMatch(etag);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


 
        static T ExposeException<T>(Optional<T> optional, string message)
        {
            if (message == null) throw new ArgumentNullException(@"message");
            if (!optional.HasValue)
            {

                throw new InvalidOperationException(message);
            }
            return optional.Value;
        }
    }
}