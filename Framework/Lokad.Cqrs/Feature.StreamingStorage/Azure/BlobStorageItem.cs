#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Net;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.StreamingStorage.Azure
{
	/// <summary>
	/// Azure BLOB implementation of the <see cref="IStorageItem"/>
	/// </summary>
	public sealed class BlobStorageItem : IStorageItem
	{
		readonly CloudBlob _blob;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageItem"/> class.
		/// </summary>
		/// <param name="blob">The BLOB.</param>
		public BlobStorageItem(CloudBlob blob)
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
		public long Write(Action<Stream> writer, StorageCondition condition, StorageWriteOptions writeOptions)
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
						throw StorageErrors.IntegrityFailure(this, ex);
					default:
						throw;
				}
			}
			catch (StorageClientException ex)
			{
				switch (ex.ErrorCode)
				{
					case StorageErrorCode.ConditionFailed:
						throw StorageErrors.ConditionFailed(this, condition, ex);
					case StorageErrorCode.ContainerNotFound:
						throw StorageErrors.ContainerNotFound(this, ex);
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
		/// <exception cref="StorageItemNotFoundException">if the item does not exist.</exception>
		/// <exception cref="StorageContainerNotFoundException">if the container for the item does not exist</exception>
		/// <exception cref="StorageItemIntegrityException">when integrity check fails</exception>
		public void ReadInto(ReaderDelegate reader, StorageCondition condition)
		{
			try
			{
				var mapped = Map(condition);
				BlobStorageUtil.Read(mapped, _blob, reader);
			}
			catch (StorageItemIntegrityException e)
			{
				throw StorageErrors.IntegrityFailure(this, e);
			}
			catch (StorageClientException e)
			{
				switch (e.ErrorCode)
				{
					case StorageErrorCode.ContainerNotFound:
						throw StorageErrors.ContainerNotFound(this, e);
					case StorageErrorCode.ResourceNotFound:
					case StorageErrorCode.BlobNotFound:
						throw StorageErrors.ItemNotFound(this, e);
					case StorageErrorCode.ConditionFailed:
						throw StorageErrors.ConditionFailed(this, condition, e);
					case StorageErrorCode.ServiceIntegrityCheckFailed:
						throw StorageErrors.IntegrityFailure(this, e);
					case StorageErrorCode.BadRequest:
						switch (e.StatusCode)
						{
								// for some reason Azure Storage happens to get here as well
							case HttpStatusCode.PreconditionFailed:
							case HttpStatusCode.NotModified:
								throw StorageErrors.ConditionFailed(this, condition, e);
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
		public void Delete(StorageCondition condition)
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
						throw StorageErrors.ContainerNotFound(this, ex);
					case StorageErrorCode.BlobNotFound:
					case StorageErrorCode.ConditionFailed:
						return;
					default:
						throw;
				}
			}
		}

		public Maybe<StorageItemInfo> GetInfo(StorageCondition condition)
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
						return Maybe<StorageItemInfo>.Empty;
					case StorageErrorCode.BadRequest:
						switch (e.StatusCode)
						{
							case HttpStatusCode.PreconditionFailed:
								return Maybe<StorageItemInfo>.Empty;
							default:
								throw;
						}
				}
				throw;
			}
		}

		public void CopyFrom(IStorageItem sourceItem,
			StorageCondition condition,
			StorageCondition copySourceCondition,
			StorageWriteOptions writeOptions)
		{
			var item = sourceItem as BlobStorageItem;

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
							throw StorageErrors.ItemNotFound(this, e);
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
						sourceItem.ReadInto((props, stream) => StreamUtil.BlockCopy(stream, targetStream, bufferSize), copySourceCondition), condition,
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
			get { return _blob;}
		}

		static BlobRequestOptions Map(StorageCondition condition,
			StorageCondition copySourceAccessCondition = default(StorageCondition))
		{
			if ((condition.Type == StorageConditionType.None) && (copySourceAccessCondition.Type == StorageConditionType.None))
				return null;

			return new BlobRequestOptions
				{
					AccessCondition = MapCondition(condition),
					CopySourceAccessCondition = MapCondition(copySourceAccessCondition)
				};
		}

		static AccessCondition MapCondition(StorageCondition condition)
		{
			switch (condition.Type)
			{
				case StorageConditionType.None:
					return AccessCondition.None;
				case StorageConditionType.IfUnmodifiedSince:
					var d1 = condition.LastModifiedUtc.ExposeException("'LastModifiedUtc' should be present.");
					return AccessCondition.IfNotModifiedSince(d1);
				case StorageConditionType.IfMatch:
					var x = condition.ETag.ExposeException("'ETag' should be present");
					return AccessCondition.IfMatch(x);
				case StorageConditionType.IfModifiedSince:
					var utc = condition.LastModifiedUtc.ExposeException("'LastModifiedUtc' should be present.");
					return AccessCondition.IfModifiedSince(utc);
				case StorageConditionType.IfNoneMatch:
					var etag = condition.ETag.ExposeException("'ETag' should be present");
					return AccessCondition.IfNoneMatch(etag);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}