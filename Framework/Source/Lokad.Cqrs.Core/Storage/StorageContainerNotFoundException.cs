using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs
{
	[Serializable]
	public class StorageContainerNotFoundException : StorageBaseException
	{
		

		public StorageContainerNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected StorageContainerNotFoundException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}
	}
}