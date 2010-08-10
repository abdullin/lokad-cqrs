using System.IO;

namespace Lokad.Cqrs.Storage
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}