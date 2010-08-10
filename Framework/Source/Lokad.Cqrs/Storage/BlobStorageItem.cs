using System;
using System.IO;
using System.Net;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Storage
{
	public sealed class BlobStorageItem : IStorageItem
	{
		readonly CloudBlob _blob;

		public BlobStorageItem(CloudBlob blob)
		{
			_blob = blob;
		}

		static BlobRequestOptions Map(StorageCondition condition, StorageCondition copySourceAccessCondition = default(StorageCondition))
		{
			if ((condition.Type == StorageConditionType.None) && (copySourceAccessCondition.Type == StorageConditionType.None))
				return null;

			return new BlobRequestOptions()
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

		StorageItemInfo Map(BlobProperties props)
		{
			return new StorageItemInfo(props.LastModifiedUtc, props.ETag);
		}

		

		public void Write(Action<Stream> writer, StorageCondition condition)
		{
			try
			{
				var options = Map(condition);

				
				
				//using (var stream = _blob.OpenWrite(options))
				//{
				//    writer(stream);
				//}

				using (var memory = new MemoryStream())
				{
					writer(memory);
					
					_blob.UploadByteArray(memory.ToArray(), options);
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


		public void ReadInto(ReaderDelegate reader, StorageCondition condition)
		{
			try
			{
				var options = Map(condition);
				// since access is lazy, we must fail and return empty condition
				// when condition is not met.

				var buffer = _blob.DownloadByteArray(options);
				using (var stream = new MemoryStream(buffer))
				{
					var properties = Map(_blob.Properties);
					reader(properties, stream);
				}
				

				//using (var stream = _blob.OpenRead(options))
				//{
				//    // we need to start pumping in order to get the properties pulled
				//    stream.ReadByte();
				//    stream.Seek(0, SeekOrigin.Begin);

				//    Enforce.That(_blob.Properties.LastModifiedUtc != DateTime.MinValue);
				//    var properties = Map(_blob.Properties);
				//    reader(properties, stream);
				//}
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

		public void Delete(StorageCondition condition)
		{
			try
			{
				_blob.Delete(Map(condition));

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

		public IStorageItem CopyFrom(IStorageItem sourceItem, 
			StorageCondition condition,
			StorageCondition copySourceCondition)
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
				Write(targetStream => sourceItem.ReadInto((props, stream) => stream.PumpTo(targetStream, bufferSize), copySourceCondition), condition);
				
			}
			return this;
		}

		public string FullPath
		{
			get { return _blob.Uri.ToString(); }
		}
	}
}