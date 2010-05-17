#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Specialized;

namespace CloudBus.Queue
{
	public sealed class IncomingMessage
	{
		public readonly NameValueCollection Headers;
		public readonly object Message;
		public readonly string Sender;
		public readonly string Topic;
		public readonly string TransportMessageId;


		public IncomingMessage(object message, IncomingMessageEnvelope envelope)
		{
			Message = message;
			TransportMessageId = envelope.TransportMessageId;
			Headers = envelope.Headers;

			Topic = envelope.Topic;
			Sender = envelope.Sender;
		}
	}
}