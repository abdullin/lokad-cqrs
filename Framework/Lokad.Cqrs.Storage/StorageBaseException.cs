using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs
{
	[Serializable]
	public class StorageBaseException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public StorageBaseException()
		{
		}

		public StorageBaseException(string message) : base(message)
		{
		}

		public StorageBaseException(string message, Exception inner) : base(message, inner)
		{
		}

		protected StorageBaseException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}