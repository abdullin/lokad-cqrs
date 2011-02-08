using System;
using System.Runtime.Serialization;

namespace Lokad.Storage
{
	[Serializable]
	public class StorageItemNotFoundException : StorageBaseException
	{
		public StorageItemNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}


		protected StorageItemNotFoundException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}
	}
}