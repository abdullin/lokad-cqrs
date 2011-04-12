using System.IO;

namespace Lokad.Cqrs.Feature.SimpleStorage
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}