#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using ProtoBuf;
using System.Linq;

namespace Lokad.Cqrs.Queue
{
	[DataContract]
	[Serializable]
	public sealed class MessageBody
	{
		[DataMember(Order = 1)]public readonly MessagePart[] Parts;

		MessageBody()
		{
		}

		public MessageBody(IEnumerable<MessagePart> parts)
		{
			Parts = parts.ToArray();
		}
	}
}