using System;
using System.Collections.Specialized;

namespace Bus2.Queue
{
	public sealed class OutgoingMessageEnvelope
	{
		Action<NameValueCollection> _headers = collection => { };

		public static OutgoingMessageEnvelope Build(Action<OutgoingMessageEnvelope> build)
		{
			var end = new OutgoingMessageEnvelope();
			build(end);
			return end;
		}

		public string Topic
		{
			set { _headers += nv => nv["topic"] = value; }
		}

		public string Sender
		{
			set { _headers += nv => nv["sender"] = value; }
		}
		
		public Guid Id
		{
			set { _headers += nv => nv["id"] = value.ToString(); }
		}

		public void CopyHeaders(NameValueCollection headers)
		{
			_headers(headers);
		}
	}
}