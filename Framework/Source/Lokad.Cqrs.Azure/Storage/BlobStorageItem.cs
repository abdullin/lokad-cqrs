#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Storage
{
	/// <summary>
	/// Azure BLOB implementation of the <see cref="IStorageItem"/>
	/// </summary>
	public sealed class BlobStorageItem : IStorageItem
	{
		readonly string _name;
		readonly CloudBlob _blob;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageItem"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="blob">The BLOB.</param>
		public BlobStorageItem(string name, CloudBlob blob)
		{
			_name = name;
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
				var compressIfPossible = (writeOptions & StorageWriteOptions.CompressIfPossible) ==
					StorageWriteOptions.CompressIfPossible;

				// we are adding our own hashing on top, to ensure
				// consistent behavior between Azure StorageClient versions

				if (compressIfPossible)
				{
					long position;
					var md5 = MD5.Create();
					_blob.Properties.ContentEncoding = "gzip";
					using (var stream = _blob.OpenWrite(mapped))
					{
						using (var crypto = new CryptoStream(stream, md5, CryptoStreamMode.Write))
						using (var compress = crypto.Compress(true))
						{
							writer(compress);
						}
						position = stream.Position;
					}
					//_blob.Properties.ContentEncoding = "gzip";
					_blob.Metadata[LokadHashFieldName] = Convert.ToBase64String(md5.Hash);
					_blob.SetMetadata();
					return position;
				}
				{
					var md5 = MD5.Create();
					long position;
					using (var stream = _blob.OpenWrite(mapped))
					{
						using (var crypto = new CryptoStream(stream, md5, CryptoStreamMode.Write))
						{
							writer(crypto);
						}
						
						position = stream.Position;
					}
					_blob.Metadata[LokadHashFieldName] = Convert.ToBase64String(md5.Hash);
					_blob.SetMetadata();
					return position;
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

		static void ReadAndVerifyHash(Stream stream, Action<Stream> reader, string hash)
		{
			if (string.IsNullOrEmpty(hash))
			{
				reader(stream);
				return;
			}

			var md5 = MD5.Create();
			// Blob streams throw NSE on Flush()
			using (var hack = new SuppressFlushForStream(stream))
			using (var crypto = new CryptoStream(hack, md5, CryptoStreamMode.Read))
			{
				reader(crypto);
			}
			var calculated = Convert.ToBase64String(md5.Hash);

			if (calculated != hash)
				throw new StorageItemIntegrityException("Hash was provided, but it does not match.");
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
				_blob.FetchAttributes(mapped);
				var props = MapFetchedAttrbitues(_name, FullPath, _blob);

				var compression = _blob.Properties.ContentEncoding ?? "";
				var md5 = _blob.Metadata[LokadHashFieldName];

				switch (compression)
				{
					case "gzip":
						using (var stream = _blob.OpenRead(mapped))
						{
							ReadAndVerifyHash(stream, s =>
								{
									// important is not to flush the decompression stream
									using (var decompress = s.Decompress(true))
									{
										reader(props, decompress);
									}
									
								}, md5);
						}

						break;
					case "":
						using (var stream = _blob.OpenRead(mapped))
						{
							ReadAndVerifyHash(stream, s => reader(props, s), md5);
						}
						break;
					default:
						throw Errors.InvalidOperation("Unsupported ContentEncoding '{0}'", compression);
				}
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
				return MapFetchedAttrbitues(_name, FullPath, _blob);
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
						sourceItem.ReadInto((props, stream) => stream.PumpTo(targetStream, bufferSize), copySourceCondition), condition,
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

		static StorageItemInfo MapFetchedAttrbitues(string name, string path, CloudBlob blob)
		{
			var meta = new NameValueCollection(blob.Metadata);
			var properties = new Dictionary<string,string>(5);
			var props = blob.Properties;
			if (!string.IsNullOrEmpty(props.ContentMD5))
			{
				properties["ContentMD5"] = props.ContentMD5;
			}
			if (!string.IsNullOrEmpty(props.ContentEncoding))
			{
				properties["ContentEncoding"] = props.ContentEncoding;
			}
			if (!string.IsNullOrEmpty(props.ContentType))
			{
				properties["ContentType"] = props.ContentType;
			}
			properties["BlobType"] = props.BlobType.ToString();
			properties["Length"] = props.Length.ToString();

			return new StorageItemInfo(name, path,   props.LastModifiedUtc, props.ETag, meta, properties);
		}

		public const string LokadHashFieldName = "LokadContentMD5";
	}

	sealed class SuppressFlushForStream : Stream
	{
		readonly Stream _inner;

		public SuppressFlushForStream(Stream inner)
		{
			_inner = inner;
		}

		public object GetLifetimeService()
		{
			return _inner.GetLifetimeService();
		}

		public object InitializeLifetimeService()
		{
			return _inner.InitializeLifetimeService();
		}

		public ObjRef CreateObjRef(Type requestedType)
		{
			return _inner.CreateObjRef(requestedType);
		}

		public void CopyTo(Stream destination)
		{
			_inner.CopyTo(destination);
		}

		public void CopyTo(Stream destination, int bufferSize)
		{
			_inner.CopyTo(destination, bufferSize);
		}

		public void Close()
		{
			_inner.Close();
		}

		public void Dispose()
		{
			_inner.Dispose();
		}

		public override void Flush()
		{
			// yeah, that's just the hack
			//_inner.Flush();
		}

		public IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _inner.BeginRead(buffer, offset, count, callback, state);
		}

		public int EndRead(IAsyncResult asyncResult)
		{
			return _inner.EndRead(asyncResult);
		}

		public IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _inner.BeginWrite(buffer, offset, count, callback, state);
		}

		public void EndWrite(IAsyncResult asyncResult)
		{
			_inner.EndWrite(asyncResult);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _inner.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_inner.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _inner.Read(buffer, offset, count);
		}

		public int ReadByte()
		{
			return _inner.ReadByte();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_inner.Write(buffer, offset, count);
		}

		public void WriteByte(byte value)
		{
			_inner.WriteByte(value);
		}

		public override bool CanRead
		{
			get { return _inner.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _inner.CanSeek; }
		}

		public bool CanTimeout
		{
			get { return _inner.CanTimeout; }
		}

		public override bool CanWrite
		{
			get { return _inner.CanWrite; }
		}

		public override long Length
		{
			get { return _inner.Length; }
		}

		public override long Position
		{
			get { return _inner.Position; }
			set { _inner.Position = value; }
		}

		public int ReadTimeout
		{
			get { return _inner.ReadTimeout; }
			set { _inner.ReadTimeout = value; }
		}

		public int WriteTimeout
		{
			get { return _inner.WriteTimeout; }
			set { _inner.WriteTimeout = value; }
		}
	}
}