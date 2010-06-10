#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;

namespace Lokad.Cqrs.Queue
{
	public sealed class OutgoingMessageEnvelope
	{
		Action<NameValueCollection> _headers = collection => { };

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

		public static OutgoingMessageEnvelope Build(Action<OutgoingMessageEnvelope> build)
		{
			var end = new OutgoingMessageEnvelope();
			build(end);
			return end;
		}

		public void CopyHeaders(NameValueCollection headers)
		{
			_headers(headers);
		}
	}
}