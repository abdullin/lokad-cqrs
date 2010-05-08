using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using Lokad;

namespace Bus2.Queue
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
			Headers = headers.AllKeys.Convert(k => new HeaderInfo(k, headers[k]));
		}
	}
}