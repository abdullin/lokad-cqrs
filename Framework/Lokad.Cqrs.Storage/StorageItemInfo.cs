using System;

namespace Lokad.Cqrs
{
	public sealed class StorageItemInfo
	{
		public DateTime LastModifiedUtc { get; private set; }
		public string ETag { get; private set; }

		public StorageItemInfo(DateTime lastModifiedUtc, string eTag)
		{
			LastModifiedUtc = lastModifiedUtc;
			ETag = eTag;
		}
	}
}