#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using Lokad;

namespace CloudBus.Queue
{
	[DataContract]
	[Serializable]
	public sealed class MessageMessage
	{
		[DataMember] public readonly HeaderInfo[] Headers;
		[DataMember] public readonly object Message;

		public MessageMessage(NameValueCollection headers, object message)
		{
			Message = message;
			Headers = headers.AllKeys.Convert(k => new HeaderInfo(k, headers[(string) k]));
		}
	}
}