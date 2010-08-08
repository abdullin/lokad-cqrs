using System.IO;

namespace Lokad.Cqrs
{
	public delegate void ReaderDelegate(StorageItemProperties props, Stream designationStream);
}