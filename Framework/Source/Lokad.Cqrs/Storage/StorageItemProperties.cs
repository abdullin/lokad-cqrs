using System;

namespace Lokad.Cqrs
{
	public sealed class StorageItemProperties
	{
		public DateTime LastModifiedUtc { get; private set; }
		public string ETag { get; private set; }

		public StorageItemProperties(DateTime lastModifiedUtc, string eTag)
		{
			LastModifiedUtc = lastModifiedUtc;
			ETag = eTag;
		}
	}
}