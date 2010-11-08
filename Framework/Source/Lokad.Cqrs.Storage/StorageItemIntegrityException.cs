using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Happens, when data corruption was detected. Normally retrying the operation should solve the problem
	/// </summary>
	[Serializable]
	public class StorageItemIntegrityException : StorageBaseException
	{
		public StorageItemIntegrityException()
		{
		}

		public StorageItemIntegrityException(string message) : base(message)
		{
		}

		public StorageItemIntegrityException(string message, Exception inner) : base(message, inner)
		{
		}

		protected StorageItemIntegrityException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}