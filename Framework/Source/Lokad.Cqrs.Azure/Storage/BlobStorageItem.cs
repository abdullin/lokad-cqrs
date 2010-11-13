#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Storage
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
		public void Write(Action<Stream> writer, StorageCondition condition, StorageWriteOptions writeOptions)
		{
			try
			{
				var mapped = Map(condition);

				if ((writeOptions & StorageWriteOptions.CompressIfPossible) == StorageWriteOptions.CompressIfPossible)
				{
					using (var stream = _blob.OpenWrite(mapped))
					using (var compress = stream.Compress())
					{
						_blob.Metadata["ContentCompression"] = "gzip";
						writer(compress);
					}
				}
				else
				{
					using (var stream = _blob.OpenWrite(mapped))
					{
						writer(stream);
					}
				}
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

		private const string MetadataMD5Key = "ContentMD5";

		/// <summary>
		/// Computes the MD5 content hash.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		/// <remarks>Copied from Lokad.Cloud</remarks>
		static string ComputeContentHashAndResetPosition(Stream source)
		{
			byte[] hash;
			source.Seek(0, SeekOrigin.Begin);
			using (var md5 = MD5.Create())
			{
				hash = md5.ComputeHash(source);
			}

			source.Seek(0, SeekOrigin.Begin);

			return Convert.ToBase64String(hash);
		}

		static Maybe<string> GetContentHash(CloudBlob blob)
		{
			var expectedHash = blob.Metadata[MetadataMD5Key];
			if (string.IsNullOrEmpty(expectedHash))
				return Maybe<string>.Empty;
			return expectedHash;
		}


		public void ReadInto(ReaderDelegate reader, StorageCondition condition)
		{
			try
			{
				var mapped = Map(condition);

				_blob.FetchAttributes(mapped);
				var props = Map(_blob.Properties);
				var compressed = _blob.Metadata["ContentCompression"] == "gzip";
				if (compressed)
				{
					using (var stream = _blob.OpenRead(mapped))
					using (var decompress = stream.Decompress())
					{
						reader(props, decompress);
					}
				}
				else
				{
					using (var stream = _blob.OpenRead(mapped))
					{
						reader(props, stream);
					}
				}
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

		public void Remove(StorageCondition condition)
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
				return Map(_blob.Properties);
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
						sourceItem.ReadInto((props, stream) => stream.PumpTo(targetStream, bufferSize), copySourceCondition), condition, writeOptions);
			}
		}

		public string FullPath
		{
			get { return _blob.Uri.ToString(); }
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

		static StorageItemInfo Map(BlobProperties props)
		{
			return new StorageItemInfo(props.LastModifiedUtc, props.ETag);
		}
	}
}