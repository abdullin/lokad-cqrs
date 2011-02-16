#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Storage
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