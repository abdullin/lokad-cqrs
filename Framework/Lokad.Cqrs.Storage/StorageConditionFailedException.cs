using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs
{
	[Serializable]
	public class StorageConditionFailedException : StorageBaseException
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public StorageConditionFailedException()
		{
		}

		public StorageConditionFailedException(string message) : base(message)
		{
		}

		public StorageConditionFailedException(string message, Exception inner) : base(message, inner)
		{
		}

		protected StorageConditionFailedException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}