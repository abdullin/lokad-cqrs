using System.IO;

namespace Lokad.Cqrs.SimpleStorage
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}