using System.IO;

namespace Lokad.Storage
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}