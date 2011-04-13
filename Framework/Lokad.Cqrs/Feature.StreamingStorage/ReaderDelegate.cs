using System.IO;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}