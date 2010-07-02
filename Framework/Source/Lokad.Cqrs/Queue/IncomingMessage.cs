#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Specialized;

namespace Lokad.Cqrs.Queue
{
	public sealed class IncomingMessage
	{
		
		public readonly object Message;
		
		public readonly string TransportMessageId;
		public readonly string Receipt;
		IncomingMessageEnvelope _envelope;

		public string Topic
		{
			get { return _envelope.Topic; }
		}


		public IncomingMessage(object message, IncomingMessageEnvelope envelope)
		{
			Message = message;
			_envelope = envelope;
			TransportMessageId = _envelope.Original.Id;
			Receipt = _envelope.Original.PopReceipt;
		}
	}
}