using System.IO;

namespace Lokad.Cqrs
{
	public delegate void ReaderDelegate(StorageItemInfo props, Stream designationStream);
}