using System.IO;

namespace Lokad.Cqrs.Feature.AzureStreamingStorage
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}